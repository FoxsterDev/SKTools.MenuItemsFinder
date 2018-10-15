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

    //[Serializable]
    internal partial class MenuItemsFinder
    {
        private bool _wasRemoving;
        private bool _isLoaded;
        private List<MenuItemLink> _menuItems;

        private static MenuItemsFinder _instance;
        private readonly MenuItemsFinderPreferences _prefs;
        
        private static MenuItemsFinder GetFinder()
        {
            return _instance ?? new MenuItemsFinder();
        }
       
        private MenuItemsFinder()
        {
            _prefs = new MenuItemsFinderPreferences();
            _prefs.Load();
        }

        private string FilterMenuItems
        {
            get { return _prefs.FilterString; }
            set
            {
                _prefs.FilterString = value;
                
                if (!_prefs.FilterString.Equals(_prefs.PreviousFilterString))
                {
                    var key = _prefs.FilterString.ToLower();
                    _prefs.PreviousFilterString = _prefs.FilterString;

                    foreach (var item in _menuItems)
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
                _menuItems = FindAllMenuItems(_prefs.CustomizedMenuItems);
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
                var customizedItems = _menuItems.FindAll(i => i.IsCustomized).ToList();
                customizedItems.ForEach(item=> { if (item.CustomHotKeys.Count > 0)
                {
                    item.CustomHotKeys.RemoveAll(h => !h.IsVerified);
                }});
                
                _prefs.CustomizedMenuItems = new List<MenuItemLink>(customizedItems);
                _prefs.Save();
            }
            catch
            {
            }
        }
        
        private void CleanRemovedItems()
        {
            if (_wasRemoving)
            {
                _wasRemoving = false;
                _menuItems = _menuItems.FindAll(i => !i.IsRemoved);
            }
        }
        
        private void UnSelectItem()
        {
            _selectedMenuItem.CustomHotKeys.RemoveAll(i => !i.IsVerified);
            _selectedMenuItem = null;
            _selectedMenuItemCustomHotKeysEditable = null;
        }

        private void SelectItem(MenuItemLink item)
        {
            _selectedMenuItem = item;
            _selectedMenuItem.CustomNameEditable = _selectedMenuItem.CustomName;
            _selectedMenuItemCustomHotKeysEditable =
                new ReorderableList(item.CustomHotKeys, typeof(MenuItemHotKey), true, true, true, true);
         
        }
        
        private void OpenAssemblyLocationThatContainsMenuItem(MenuItemLink item)
        {
            var directoryPath = new FileInfo(item.DeclaringType.Assembly.Location).DirectoryName;
            OpenFile(directoryPath);
        }

    }
}