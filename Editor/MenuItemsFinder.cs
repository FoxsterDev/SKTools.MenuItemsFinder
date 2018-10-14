using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Assertions;
using Debug = UnityEngine.Debug;

namespace SKTools.MenuItemsFinder
{
    internal partial class MenuItemsFinder : IDisposable
    {
        private static Dictionary<string, string> _hotKeysMap;
        
        private bool _wasRemoving = false;
        public MenuItemLink SelectedMenuItem;
        public List<MenuItemLink> MenuItems;
        public ReorderableList SelectedMenuItemCustomHotKeysEditable;
      
        public Texture2D StarredImage, UnstarredImage, LoadingImage, SettingsImage;

        public MenuItemsFinder()
        {
            Debug.Log(typeof(MenuItemsFinder).Name + ", version=" + MenuItemsFinderVersion.Version);
        }
        
        public MenuItemsFinderPreferences Prefs
        {
            get { return MenuItemsFinderPreferences.Current; }
        }

     
   
        public string FilterString
        {
            get { return Prefs.FilterString; }
            set
            {
                Prefs.FilterString = value;
                
                if (!Prefs.FilterString.Equals(Prefs.PreviousFilterString))
                {
                    var key = Prefs.FilterString.ToLower();
                    Prefs.PreviousFilterString = Prefs.FilterString;

                    foreach (var item in MenuItems)
                    {
                        item.IsFiltered = string.IsNullOrEmpty(key) ||
                                          item.Key.Contains(key) ||
                                          (!string.IsNullOrEmpty(item.CustomName) &&
                                           item.CustomName.ToLower().Contains(key));
                    }
                }
            }
        }
        
        public void SavePrefs()
        {
            try
            {
                var customizedItems = MenuItems.FindAll(i => i.IsCustomized).ToList();
                customizedItems.ForEach(item=> { if (item.CustomHotKeys.Count > 0)
                {
                    item.CustomHotKeys.RemoveAll(h => !h.IsVerified);
                }});
                
                Prefs.CustomizedMenuItems = new List<MenuItemLink>(customizedItems);
                Prefs.Save();
            }
            catch
            {
            }
        }
     
        public void Load()
        {
            try
            {
                var assetPath = Prefs.AssetsPath;
                
                UnstarredImage = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath + "unstarred.png");
                StarredImage = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath + "starred.png");
                LoadingImage = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath + "loading.png");
                SettingsImage = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath + "settings.png");
                
                MenuItems = FindAllMenuItems(Prefs.CustomizedMenuItems);
                
                Assert.IsNotNull(StarredImage, "Check path=" +assetPath + "starred.png");
                Assert.IsNotNull(UnstarredImage, "Check path=" + assetPath + "unstarred.png");
            }
            catch (Exception ex)
            {
                Debug.LogError("Cant load prefs=" + ex);
            }
        }

        public void AddCustomizedNameToPrefs(MenuItemLink link)
        {
            if (SelectedMenuItem != null && !string.IsNullOrEmpty(SelectedMenuItem.CustomNameEditable))
            {
                link.CustomName = SelectedMenuItem.CustomNameEditable;
                link.UpdateLabel();
            }
        }

        public void MarkAsRemoved(MenuItemLink item)
        {
            _wasRemoving = true;
            item.IsRemoved = true;
        }

        public void CleanRemovedItems()
        {
            if (_wasRemoving)
            {
                _wasRemoving = false;
                MenuItems = MenuItems.FindAll(i => !i.IsRemoved);
            }
        }

        public bool ToggleSettings(MenuItemLink item)
        {
            if (SelectedMenuItem == null)
            {
                ShowSettings(item);
                return true;
            }

            var newSettings = SelectedMenuItem.Key != item.Key;
            HideSettings();

            if (newSettings)
            {
                ShowSettings(item);
                return true;
            }

            return false;
        }

        private void HideSettings()
        {
            SelectedMenuItem.CustomHotKeys.RemoveAll(i => !i.IsVerified);
            SelectedMenuItem = null;
            SelectedMenuItemCustomHotKeysEditable = null;
        }

        private void ShowSettings(MenuItemLink item)
        {
            SelectedMenuItem = item;
            SelectedMenuItem.CustomNameEditable = SelectedMenuItem.CustomName;
            SelectedMenuItemCustomHotKeysEditable =
                new ReorderableList(item.CustomHotKeys, typeof(MenuItemHotKey), true, true, true, true);
         
        }

       

        public void ToggleStarred(MenuItemLink item)
        {
            item.Starred = !item.Starred;
        }

        public void OpenAssemblyLocationThatContainsMenuItem(MenuItemLink item)
        {
            var directoryPath = new FileInfo(item.DeclaringType.Assembly.Location).DirectoryName;
            OpenFile(directoryPath);
        }

        public void TryOpenFileThatContainsMenuItem(MenuItemLink item, out string error)
        {
            error = "";
            //Unity.TextMeshPro.Editor
            var assemblyFilePath = Path.GetFileNameWithoutExtension(item.AssemlyFilePath);
            Debug.Log(assemblyFilePath);

            try
            {
                var assemblyCprojPath = Application.dataPath.Replace("Assets", assemblyFilePath + ".csproj");
                if (!File.Exists(assemblyCprojPath))
                {
                    error = "cant detect file from " + assemblyFilePath + ".dll in assets folder\n";
                    return;
                }

                var lines = File.ReadAllLines(assemblyCprojPath);
                var assemblyFiles = new List<string>();
                var infoFound = false;

                foreach (var line in lines)
                {
                    if (line.Contains("<Compile Include="))
                    {
                        infoFound = true;
                        assemblyFiles.Add(line.Split('"')[1]);
                    }
                    else if (infoFound)
                    {
                        break;
                    }
                }

                var itemDeclaringTypeName = item.DeclaringType.Name;

                foreach (var assetPath in assemblyFiles)
                {
                    if (assetPath.Contains(itemDeclaringTypeName)
                    ) //suppose that type and file name equals about another cases need to think
                    {
                        var fullPath = Path.Combine(Application.dataPath, assetPath.Substring(7));
                        OpenFile(fullPath);
                        EditorGUIUtility.systemCopyBuffer = item.Path;
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            error = "cant detect file from " + assemblyFilePath + ".dll in assets folder\n";
            ///Users/sergeykha/Library/Unity/cache/packages/packages.unity.com/com.unity.textmeshpro@1.2.4/Scripts/Editor
        }

        private void OpenFile(string filePath)
        {
#if !UNITY_EDITOR_WIN
            filePath = "file://" + filePath.Replace(@"\", "/");
#else
            filePath = @"file:\\" + filePath.Replace("/", @"\");;
#endif
            Application.OpenURL(filePath);
        }
          
        public void Dispose()
        {
            Resources.UnloadAsset(StarredImage);
            Resources.UnloadAsset(UnstarredImage);
            Resources.UnloadAsset(LoadingImage);
            Resources.UnloadAsset(SettingsImage);
        }
    }
}