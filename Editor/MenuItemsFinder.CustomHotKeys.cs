using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SKTools.MenuItemsFinder
{
    internal partial class MenuItemsFinder
    {
        private static FieldInfo _eventInfo;
        
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
        
        [InitializeOnLoadMethod]
        private static void MenuItemsFinder_CustomHotkeys_Initializer()
        {
            _eventInfo = typeof(Event).GetField("s_Current", BindingFlags.Static | BindingFlags.NonPublic);
            
            EditorApplication.update += Update;
        }

        private static void Update()
        {
            var ev = (Event) _eventInfo.GetValue(null);
            var id = GUIUtility.GetControlID(FocusType.Passive);
            var eventType = ev.GetTypeForControl(id);
        
            if (eventType == EventType.KeyUp)
            {
                if (TryExecuteHotKey(ev))
                {
                    ev.Use();
                }
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
        
        public bool TryAddHotkeyToSelectedItem(MenuItemLink menuItem, MenuItemHotKey hotkey, out string error)
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
            
            var exist = MenuItems.Find(i => i.Key != menuItem.Key && ((i.HotKey !=null && i.HotKey.Equals(hotkey)) || i.CustomHotKeys.Contains(hotkey)));
            if (exist != null)
            {
                error = exist.Path + " this menuitem already contains hotkey " + hotkey;
                return false;
            }

            hotkey.IsVerified = true;
            
            menuItem.UpdateLabel();
            return true;
        }
    }
}