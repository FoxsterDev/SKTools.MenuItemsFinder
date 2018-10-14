using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SKTools.MenuItemsFinder
{
    internal partial class MenuItemsFinder
    {
        private static Dictionary<string, string> HotKeysMap
        {
            get
            {
                if (_hotKeysMap == null)
                {
                    _hotKeysMap = new Dictionary<string, string>();
                    foreach (var item in MenuItemsFinderPreferences.Current.CustomizedMenuItems)
                    {
                        foreach (var hotKey in item.CustomHotKeys)
                        {
                            _hotKeysMap[hotKey] = item.Path;
                        }
                    }
                }

                return _hotKeysMap;
            }
        }
        
        private static bool TryExecuteHotKey(Event ev)
        {
            var inputHotkey = ConvertEventToHotKey(ev);
            
            if (inputHotkey != null)
            {
                string menuItemPath;
                HotKeysMap.TryGetValue(inputHotkey, out menuItemPath);

                if (!string.IsNullOrEmpty(menuItemPath))
                {
                    return EditorApplication.ExecuteMenuItem(menuItemPath);
                }
            }

            return false;
        }
        
        private static MenuItemHotKey ConvertEventToHotKey(Event ev)
        {
            if (ev.isKey && (ev.shift || ev.alt || ev.command || ev.control))
            {
                return new MenuItemHotKey
                {
                    Alt = ev.alt,
                    Shift = ev.shift,
                    Cmd = ev.command || ev.control,
                    Key = ev.keyCode.ToString().ToLower()
                };
            }

            return null;
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
            
            SelectedMenuItem.UpdateLabel();
            return true;
        }
    }
}