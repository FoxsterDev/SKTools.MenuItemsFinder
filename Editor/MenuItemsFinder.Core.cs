using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace SKTools.MenuItemsFinder
{
    internal partial class MenuItemsFinder 
    {
        private static MenuItemsFinder _instance;
        private readonly MenuItemsFinderPreferences _prefs;
        private bool _isLoaded;
        private List<MenuItemLink> _menuItems;

        private MenuItemsFinder()
        {
            _prefs = new MenuItemsFinderPreferences();
            _prefs.Load();
        }

        private static MenuItemsFinder GetFinder()
        {
            if (_instance == null)
            {
                _instance = new MenuItemsFinder();
                _instance.LoadMenuItems();
                return _instance;
            }
            return _instance;
        }
        
        private bool IsCustomized(MenuItemLink item)
        {
            return item.Starred || !string.IsNullOrEmpty(item.CustomName) || item.IsMissed || item.CustomHotKeys.Count > 0;
        }

        private void LoadMenuItems()
        {
            try
            {
                _menuItems = GetMenuItems(_prefs.CustomizedMenuItems);
                _isLoaded = true;
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
                _prefs.CustomizedMenuItems = _menuItems.FindAll(IsCustomized).ToList();
                _prefs.CustomizedMenuItems.ForEach(item =>
                {
                    if (item.CustomHotKeys.Count > 0) item.CustomHotKeys.RemoveAll(h => !h.IsVerified);
                });

                _prefs.Save();
            }
            catch
            {
            }
        }
        
        private List<MenuItemLink> GetMenuItems(List<MenuItemLink> customizedItems)
        {
            var watch = new Stopwatch();
            watch.Start();

            var menuItemData = LoadMenuItemData();

            var menuItemsLinksDict = new Dictionary<string, MenuItemLink>(menuItemData.Count);
            
            var menuItemLinks = CreateMenuItemLinks(menuItemData, menuItemsLinksDict);
            CustomizeMenuItems(menuItemLinks, customizedItems, menuItemsLinksDict);
         
            watch.Stop();
            
            Debug.Log("FindAllMenuItems takes=" + watch.ElapsedMilliseconds +  "ms, Count=" + menuItemLinks.Count); //for mac book pro 2018 it takes about 170 ms, it is not critical affects every time to run it

            menuItemLinks.Sort((left, right) => left.Path[0] - right.Path[0]);
            return menuItemLinks;
        }

        private void CustomizeMenuItems(List<MenuItemLink> menuItems, List<MenuItemLink> customizedItems,  Dictionary<string, MenuItemLink> menuItemLinksDict)
        {
            MenuItemLink menuItemLink;
            foreach (var customized in customizedItems)
            {
                if (string.IsNullOrEmpty(customized.Path))
                    continue;

                menuItemLinksDict.TryGetValue(customized.Path, out menuItemLink);
                
                if (menuItemLink == null)
                {
                    customized.UpdateLabel();
                    menuItems.Add(customized);
                    continue;
                }

                menuItemLink.UpdateFrom(customized);
                menuItemLink.UpdateHotKeys(customized);
            }
        }

        private List<MenuItemLink> CreateMenuItemLinks(Dictionary<string, MenuItemData> menuItemData, Dictionary<string, MenuItemLink> menuItemLinksDict)
        {
            var menuItems = new List<MenuItemLink>(menuItemData.Count);
            
            foreach (var entry in menuItemData)
            {
                if (entry.Value.TargetMethodValidate != null && entry.Value.TargetMethod == null)
                {
                    Debug.LogWarning("There is a validate method without execution method=" +
                                     entry.Value.TargetMethodValidate.Name + " menupath=" +
                                     entry.Value.TargetAttributeValidate.menuItem);
                    continue;
                }

                var link = new MenuItemLink(entry.Value);
                link.UpdateHotKeys(null);
                menuItems.Add(link);
                menuItemLinksDict[link.Path] = link;
            }

            return menuItems;
        }
        
        private Dictionary<string, MenuItemData> LoadMenuItemData()
        {
            var dict = new Dictionary<string, MenuItemData>(200);
            
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes();
                
                foreach (var type in types)
                {
                    var methods = type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

                    foreach (var method in methods)
                    {
                        ExtractMenuItem(method, dict);
                    }
                }
            }

            return dict;
        }

        private void ExtractMenuItem(MethodInfo method, Dictionary<string, MenuItemData> dict)
        {
            var items = method.GetCustomAttributes(typeof(MenuItem), false).Cast<MenuItem>().ToArray();
            if (items.Length != 1)
            {
                return;
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