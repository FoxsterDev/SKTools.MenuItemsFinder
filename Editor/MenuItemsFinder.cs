using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Assertions;
using Debug = UnityEngine.Debug;

namespace SKTools.MenuItemsFinder
{
    internal class MenuItemsFinder : IDisposable
    {
        private string _prefsFilePath;
        private bool _wasRemoving = false;

        public MenuItemLink SelectedMenuItem;
        public ReorderableList SelectedMenuItemCustomHotKeysEditable;

        public List<MenuItemLink> MenuItems; //, FilteredMenuItems = new List<MenuItemLink>();
        public Texture2D StarredImage, UnstarredImage, LoadingImage, SettingsImage;


        public MenuItemsFinderPreferences Prefs = new MenuItemsFinderPreferences();
      

        public MenuItemsFinder()
        {
            Debug.Log(typeof(MenuItemsFinder).Name + ", version=" + MenuItemsFinderVersion.Version);
        }

        public void SavePrefs()
        {
            try
            {
                Prefs.CustomizedMenuItems.Clear();
                foreach (var item in MenuItems)
                {
                    if (item.IsCustomized)
                    {
                        if (item.CustomHotKeys.Count > 0)
                        {
                            item.CustomHotKeys.RemoveAll(h => !h.IsVerified);
                        }
                        Prefs.CustomizedMenuItems.Add(item);
                    }
                }

                File.WriteAllText(_prefsFilePath, EditorJsonUtility.ToJson(Prefs, true));
            }
            catch
            {
            }
        }
     
        public void Load()
        {
            try
            {
                var stackTrace = new StackTrace(true);
                var editorDirectory = stackTrace.GetFrames()[0].GetFileName().Replace(typeof(MenuItemsFinder).Name + ".cs", string.Empty);
                
                _prefsFilePath = editorDirectory + "Prefs.json";
                
                if (File.Exists(_prefsFilePath))
                {
                    EditorJsonUtility.FromJsonOverwrite(File.ReadAllText(_prefsFilePath), Prefs);
                }

                var assetsPath = editorDirectory.Replace("Editor", "Editor Resources")
                    .Substring(Application.dataPath.Length - "Assets".Length);

                UnstarredImage = AssetDatabase.LoadAssetAtPath<Texture2D>(assetsPath + "unstarred.png");
                StarredImage = AssetDatabase.LoadAssetAtPath<Texture2D>(assetsPath + "starred.png");
                LoadingImage = AssetDatabase.LoadAssetAtPath<Texture2D>(assetsPath + "loading.png");
                SettingsImage = AssetDatabase.LoadAssetAtPath<Texture2D>(assetsPath + "settings.png");
                
                MenuItems = FindAllMenuItems(Prefs.CustomizedMenuItems);
                MenuItems.Sort((left, right) => left.Path[0] - right.Path[0]);

                Assert.IsNotNull(StarredImage, "Check path=" + assetsPath + "starred.png");
                Assert.IsNotNull(UnstarredImage, "Check path=" + assetsPath + "unstarred.png");
            }
            catch (Exception ex)
            {
                Debug.LogError("Cant load prefs=" + ex);
            }
        }

        private List<MenuItemLink> FindAllMenuItems(List<MenuItemLink> customizedItems)
        {
            var watch = new Stopwatch();
            watch.Start();

            var dict = new Dictionary<string, MenuItemData>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            var menuItems = new List<MenuItemLink>(200);
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    var methods =
                        type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic |
                                        BindingFlags.Public);

                    foreach (var method in methods)
                    {
                        var items = method.GetCustomAttributes(typeof(MenuItem), false).Cast<MenuItem>().ToArray();
                        if (items.Length != 1)
                        {
                            if (items.Length > 1)
                            {
                                Debug.LogError("11111=" + items.Length);
                            }

                            continue;
                        }

                        var key = items[0].menuItem;

                        MenuItemData data;
                        dict.TryGetValue(key, out data);
                        if (data == null)
                        {
                            data = new MenuItemData();
                            dict.Add(key, data);
                        }

                        if (items[0].validate)
                        {
                            data.TargetAttributeValidate = items[0];
                            data.TargetMethodValidate = method;
                        }
                        else
                        {
                            data.TargetAttribute = items[0];
                            data.TargetMethod = method;
                        }
                    }
                }
            }

            var dictWithKeyMenuItem = new Dictionary<string, MenuItemLink>(dict.Count);

            foreach (var entry in dict)
            {
                if (entry.Value.TargetMethodValidate != null && entry.Value.TargetMethod == null)
                {
                    Debug.LogWarning("There is a validate method without execution method=" +
                                     entry.Value.TargetMethodValidate.Name + " menupath=" +
                                     entry.Value.TargetAttributeValidate.menuItem);
                    continue;
                }

                var link = new MenuItemLink(entry.Value);
                menuItems.Add(link);
                dictWithKeyMenuItem[link.Path] = link;
            }

            MenuItemLink menuItemLink;
            foreach (var customized in customizedItems)
            {
                if (string.IsNullOrEmpty(customized.Path))
                    continue;

                dictWithKeyMenuItem.TryGetValue(customized.Path, out menuItemLink);
                if (menuItemLink == null)
                {
                    customized.UpdateLabel();
                    menuItems.Add(customized);
                    continue;
                }

                menuItemLink.UpdateFrom(customized);
            }

            watch.Stop();
            Debug.Log("Time to FindAllMenuItems takes=" + watch.ElapsedMilliseconds +
                      "ms Count=" +
                      menuItems.Count); //for mac book pro 2018 it takes about 170 ms, it is not critical affects every time to run it

            return menuItems;
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

            SelectedMenuItemCustomHotKeysEditable =
                new ReorderableList(item.CustomHotKeys, typeof(MenuItemHotKey), true, true, true, true);
         
        }

        public bool TryAddHotkeyToSelectedItem(MenuItemHotKey hotkey, out string error)
        {
            var item = default(MenuItemLink);
            error = string.Empty;
            var checkModifiers = hotkey.Alt | hotkey.Cmd | hotkey.Shift;
            if (!checkModifiers)
            {
                error = " there needs to have at least one modifier alt or cmd or shift!";
                return false;
            }

            var key = hotkey.Key.ToCharArray();
            //Debug.LogError(key[0] +" , "+ key.Length);
            //Debug.Log(key[0] >= 'a' && key[0] <= 'z');
            
            if (!(key.Length == 1 && ( ((key[0] - 42) >= 0 && (key[0] - 42) <= 9)) || (key[0] >= 'A' && key[0] <= 'Z') || (key[0] >= 'a' && key[0] <= 'z')))
            {
                error = "please use this interval of available symbols for the key A-Z, a-z, 0-9";
                return false;
            }
            
            var exist = MenuItems.Find(i => i.Key != SelectedMenuItem.Key && ((i.HotKey !=null && i.HotKey.Equals(hotkey)) || i.CustomHotKeys.Contains(hotkey)));
            if (exist != null)
            {
                error = exist.Path + " this menuitem already contains hotkey " + hotkey;
                return false;
            }

            hotkey.IsVerified = true;
            //hotkey.HotkeyString = MenuItemHotKey.ToPack(hotkey);
            //SelectedMenuItemCustomHotKeysEditable.list.Remove(hotkey);
            //SelectedMenuItem.CustomHotKeys.Add(hotkey);
            SelectedMenuItem.UpdateLabel();
            return true;
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