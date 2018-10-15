using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace SKTools.MenuItemsFinder
{
    public delegate void FinderDelegate();
    public delegate void FinderDelegate<T>(T obj);
    
    internal partial class MenuItemsFinder
    {
        private bool _wasRemoving;
        private bool _isLoaded;
        private MenuItemLink SelectedMenuItem;
        private List<MenuItemLink> MenuItems;
        
        private static MenuItemsFinder _instance;
        private MenuItemsFinderPreferences Prefs;
        
        private static MenuItemsFinder GetFinder()
        {
            return _instance ?? new MenuItemsFinder();
        }

        private MenuItemsFinder()
        {
            Prefs = new MenuItemsFinderPreferences();
            Prefs.Load();
        }

        private string FilterString
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
        
        private void Load()
        {
            try
            {
                MenuItems = FindAllMenuItems(Prefs.CustomizedMenuItems);
                _isLoaded = true;
                Debug.Log(typeof(MenuItemsFinder).Name + " was loaded, version=" + MenuItemsFinderVersion.Version);
            }
            catch (Exception ex)
            {
                Debug.LogError("[MenuItemsFinder] could not be loaded!");
            }
        }

        private void SavePrefs()
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

        private void AddCustomizedNameToPrefs(MenuItemLink link)
        {
            if (SelectedMenuItem != null && !string.IsNullOrEmpty(SelectedMenuItem.CustomNameEditable))
            {
                link.CustomName = SelectedMenuItem.CustomNameEditable;
                link.UpdateLabel();
            }
        }

        private void MarkAsRemoved(MenuItemLink item)
        {
            _wasRemoving = true;
            item.IsRemoved = true;
        }

        private void CleanRemovedItems()
        {
            if (_wasRemoving)
            {
                _wasRemoving = false;
                MenuItems = MenuItems.FindAll(i => !i.IsRemoved);
            }
        }

        private bool ToggleSettings(MenuItemLink item)
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
            _selectedMenuItemCustomHotKeysEditable = null;
        }

        private void ShowSettings(MenuItemLink item)
        {
            SelectedMenuItem = item;
            SelectedMenuItem.CustomNameEditable = SelectedMenuItem.CustomName;
            _selectedMenuItemCustomHotKeysEditable =
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
                    ) //suppose that type and file name equals about another cases need to implement
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
    }
}