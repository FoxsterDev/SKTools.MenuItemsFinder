using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using Debug = UnityEngine.Debug;

namespace SKTools.MenuItemsFinder
{
    internal class MenuItemsFinder
    {
        private string _prefsFilePath;

        public MenuItemLink RolledOutMenuItem;
        public List<MenuItemLink> MenuItems, FilteredMenuItems = new List<MenuItemLink>();
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
                    if (item.Starred || !string.IsNullOrEmpty(item.CustomName))
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

                MenuItems = FindAllMenuItems();
                MenuItems.Sort((x, y) => y.Label[0] - x.Label[0]);

                Assert.IsNotNull(StarredImage, "Check path=" + starFilePath + "starred.png");
                Assert.IsNotNull(UnstarredImage, "Check path=" + starFilePath + "unstarred.png");
            }
            catch (Exception ex)
            {
                Debug.LogError("Cant load prefs=" + ex);
            }
        }

        private List<MenuItemLink> FindAllMenuItems()
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
                dictWithKeyMenuItem[link.Key] = link;
            }

            MenuItemLink menuItemLink;
            foreach (var customized in Prefs.CustomizedMenuItems)
            {
                if(string.IsNullOrEmpty(customized.Key))
                    continue;
                dictWithKeyMenuItem.TryGetValue(customized.Key, out menuItemLink);
                if (menuItemLink == null)
                    continue;
                menuItemLink.CustomName = customized.CustomName;
                menuItemLink.Starred = customized.Starred;
                menuItemLink.UpdateLabel();
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

        public void AllUnstarred()
        {
            MenuItems.ForEach(itemLink =>
            {
                if (itemLink.Starred)
                {
                    ToggleStarred(itemLink);
                }
            });
        }

        public void ToggleStarred(MenuItemLink item)
        {
            item.Starred = !item.Starred;
        }
    }
}