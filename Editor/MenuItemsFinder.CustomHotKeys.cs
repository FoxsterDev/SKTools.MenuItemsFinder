using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace SKTools.MenuItemsFinder
{
    internal partial class MenuItemsFinder
    {
        private ReorderableList _selectedMenuItemCustomHotKeysEditable;

        private  FieldInfo _eventInfo;
        private  Dictionary<string, string> _hotKeysMap;
       
        [InitializeOnLoadMethod]
        private static void MenuItemsFinder_CustomHotkeys_Initializer()
        {
            GetFinder().CustomHotKeysInitializer();
        }

        private void CustomHotKeysInitializer()
        {
            _eventInfo = typeof(Event).GetField("s_Current", BindingFlags.Static | BindingFlags.NonPublic);
            _hotKeysMap = new Dictionary<string, string>(10);
            
            foreach (var item in Prefs.CustomizedMenuItems)
            {
                foreach (var hotKey in item.CustomHotKeys)
                {
                    _hotKeysMap[hotKey] = item.Path;
                }
            }
            
            EditorApplication.update += KeyboardInputUpdate;
        }
        
        private void KeyboardInputUpdate()
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
        
        private bool TryExecuteHotKey(Event ev)
        {
            var inputHotkey = ConvertEventToHotKey(ev);
            
            if (inputHotkey != null)
            {
                string menuItemPath;
                _hotKeysMap.TryGetValue(inputHotkey, out menuItemPath);

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

        private bool TryAddHotkeyToSelectedItem(MenuItemLink menuItem, MenuItemHotKey hotkey, out string error)
        {
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