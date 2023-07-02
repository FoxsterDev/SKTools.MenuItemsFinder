using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using SKTools.Core.Editor;
using UnityEditor;
using UnityEngine;

namespace SKTools.MenuItemsFinder
{
    internal partial class MenuItemsFinder
    {
        private static MenuItemsFinder _instance;

        private readonly Preferences _prefs;
        private readonly MenuItemsFinderStyles _styles;
        private bool _isLoaded;
        private List<MenuItemLink> _menuItems;
        private bool _wasItemsRemoving;

        private MenuItemsFinder()
        {
            _prefs = new Preferences();
            _prefs.Load();

            var settingsPath = Utility.GetAssetPathRelativeToCurrentDirectory("Editor Resources", "Styles.asset");
            _styles = AssetDatabase.LoadAssetAtPath<MenuItemsFinderStyles>(settingsPath);

            if (_styles == null)
            {
                _styles = ScriptableObject.CreateInstance<MenuItemsFinderStyles>();
                Debug.LogWarning("cant load settings from path=" + settingsPath);
            }
        }

        private string FilterMenuItems
        {
            get { return _prefs.FilterString; }
            set
            {
                _prefs.FilterString = value;

                if (_prefs.FilterString != _prefs.PreviousFilterString)
                {
                    var key = !string.IsNullOrEmpty(_prefs.FilterString)
                                  ? _prefs.FilterString.ToLower()
                                  : string.Empty;
                    _prefs.PreviousFilterString = _prefs.FilterString = key;

                    SetFilteredItems(key);
                }
            }
        }

        private static MenuItemsFinder GetFinder()
        {
            return _instance ?? (_instance = new MenuItemsFinder());
        }

        private bool IsCustomized(MenuItemLink item)
        {
            return item.Starred || !string.IsNullOrEmpty(item.CustomName) || item.IsMissed ||
                   item.CustomHotKeys.Count > 0;
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
                Debug.LogError("[MenuItemsFinder] could not be loaded!\n" + ex);
            }
        }

        private void SavePrefs()
        {
            try
            {
                _prefs.CustomizedMenuItems = _menuItems.FindAll(IsCustomized).ToList();
                _prefs.CustomizedMenuItems.ForEach(
                    item =>
                    {
                        if (item.CustomHotKeys.Count > 0)
                        {
                            item.CustomHotKeys.RemoveAll(h => !h.IsVerified);
                        }
                    });

                _prefs.Save();
            }
            catch
            {
                // ignored
            }
        }

        private List<MenuItemLink> GetMenuItems(List<MenuItemLink> customizedItems)
        {
            var menuItemData = LoadMenuItemData();
            var menuItemsLinksDict = new Dictionary<string, MenuItemLink>(menuItemData.Count);
            var menuItemLinks = CreateMenuItemLinks(menuItemData, menuItemsLinksDict);

            CustomizeMenuItems(menuItemLinks, customizedItems, menuItemsLinksDict);

            menuItemLinks.ForEach(UpdateLabel);
            menuItemLinks.Sort((left, right) => left.Key[0] - right.Key[0]);

            return menuItemLinks;
        }

        private void UpdateLabel(MenuItemLink item)
        {
            item.UpdateLabel();
            item.UpdateLabelWithHotKey(_styles.ItemHotKeyColor);
        }

        private void CustomizeMenuItems(
            List<MenuItemLink> menuItems, List<MenuItemLink> customizedItems,
            Dictionary<string, MenuItemLink> menuItemLinksDict)
        {
            MenuItemLink menuItem;
            foreach (var customizedItem in customizedItems)
            {
                if (string.IsNullOrEmpty(customizedItem.Key))
                {
                    continue;
                }

                menuItemLinksDict.TryGetValue(customizedItem.Key, out menuItem);

                if (menuItem == null)
                {
                    menuItems.Add(customizedItem);
                    continue;
                }

                menuItem.UpdateFrom(customizedItem);
                menuItem.UpdateCustomHotKeysFrom(customizedItem);
            }
        }

        private List<MenuItemLink> CreateMenuItemLinks(
            Dictionary<string, MenuItemData> menuItemData,
            Dictionary<string, MenuItemLink> menuItemLinksDict)
        {
            var menuItems = new List<MenuItemLink>(menuItemData.Count);

            foreach (var entry in menuItemData)
            {
                if (entry.Value.TargetMethodValidate != null && entry.Value.TargetMethod == null)
                {
                    Debug.LogWarning(
                        "There is a validate method without execution method=" +
                        entry.Value.TargetMethodValidate.Name + " menupath=" +
                        entry.Value.TargetAttributeValidate.menuItem);
                    continue;
                }

                var item = new MenuItemLink(entry.Value);
                item.UpdateOriginalHotKey();

                menuItems.Add(item);
                menuItemLinksDict[item.Key] = item;
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

        private void OpenAssemblyLocationThatContainsMenuItem(MenuItemLink item)
        {
            var directoryPath = new FileInfo(item.DeclaringType.Assembly.Location).DirectoryName;
            Utility.OpenFile(directoryPath);
        }

        private void SetFilteredItems(string key)
        {
            foreach (var item in _menuItems)
            {
                item.IsFiltered = string.IsNullOrEmpty(key) ||
                                  !string.IsNullOrEmpty(item.Key) && item.Key.Contains(key) ||
                                  !string.IsNullOrEmpty(item.CustomName) &&
                                  item.CustomName.ToLower().Contains(key);
            }
        }

        private void CleanRemovedItems()
        {
            if (_wasItemsRemoving)
            {
                _wasItemsRemoving = false;
                _menuItems = _menuItems.FindAll(i => !i.IsRemoved);
            }
        }
    }
}