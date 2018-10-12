﻿using System;
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

        public MenuItemLink RolledOutMenuItem;
        public ReorderableList CustomHotKeysEditable;

        public List<MenuItemLink> MenuItems; //, FilteredMenuItems = new List<MenuItemLink>();
        public Texture2D StarredImage, UnstarredImage, LoadingImage, SettingsImage;

      
        public MenuItemsFinderPreferences Prefs = new MenuItemsFinderPreferences
        {
            FilterString = "Please type menuitem name here.."
        };

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
                        Prefs.CustomizedMenuItems.Add(item);
                    }
                }

                File.WriteAllText(_prefsFilePath, EditorJsonUtility.ToJson(Prefs));
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
                var editorDirectory = stackTrace.GetFrames()[0].GetFileName()
                    .Replace(typeof(MenuItemsFinder).Name + ".cs", string.Empty);
                _prefsFilePath = editorDirectory + "Prefs.json";
                var starFilePath = editorDirectory.Replace("Editor", "Editor Resources")
                    .Substring(Application.dataPath.Length - "Assets".Length);

                UnstarredImage = AssetDatabase.LoadAssetAtPath<Texture2D>(starFilePath + "unstarred.png");
                StarredImage = AssetDatabase.LoadAssetAtPath<Texture2D>(starFilePath + "starred.png");
                LoadingImage = AssetDatabase.LoadAssetAtPath<Texture2D>(starFilePath + "loading.png");
                SettingsImage = AssetDatabase.LoadAssetAtPath<Texture2D>(starFilePath + "settings.png");

                if (File.Exists(_prefsFilePath))
                {
                    EditorJsonUtility.FromJsonOverwrite(File.ReadAllText(_prefsFilePath), Prefs);
                }

                MenuItems = FindAllMenuItems(Prefs.CustomizedMenuItems);
                MenuItems.Sort((left, right) => left.Path[0] - right.Path[0]);

                Assert.IsNotNull(StarredImage, "Check path=" + starFilePath + "starred.png");
                Assert.IsNotNull(UnstarredImage, "Check path=" + starFilePath + "unstarred.png");
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
                    customized.PostUpdate();
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
            if (RolledOutMenuItem != null && !string.IsNullOrEmpty(RolledOutMenuItem.CustomNameEditable))
            {
                link.CustomName = RolledOutMenuItem.CustomNameEditable;
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
            if (RolledOutMenuItem == null)
            {
                ShowSettings(item);
                return true;
            }

            var newSettings = RolledOutMenuItem.Key != item.Key;
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
            RolledOutMenuItem.CustomHotKeys.RemoveAll(i => !i.IsVerified);
            RolledOutMenuItem = null;
            CustomHotKeysEditable = null;
        }

        private void ShowSettings(MenuItemLink item)
        {
            RolledOutMenuItem = item;

            CustomHotKeysEditable =
                new ReorderableList(item.CustomHotKeys, typeof(MenuItemHotKey), true, true, true, true);
         
        }

        public void CheckAndAddHotkey(MenuItemHotKey hotkey, out string error)
        {
            throw new NotImplementedException();
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