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
        private FieldInfo _eventInfo;
        
        /// <summary>
        /// To prevent several hotKeysMap.
        /// In unity editor are possible to drop instance of variables state.
        /// For example not only clear recompiling, but and just open some instance editorwindow
        /// We could be create instance ScriptableObject,
        /// but it helps with only serializable type
        /// </summary>
        private static Dictionary<string, string> _hotKeysMap;

        [InitializeOnLoadMethod]
        private static void MenuItemsFinder_CustomHotkeys_Initializer()
        {
            GetFinder().CustomHotKeysInitializer();
        }

        private void CustomHotKeysInitializer()
        {
            _eventInfo = typeof(Event).GetField("s_Current", BindingFlags.Static | BindingFlags.NonPublic);

            UpdateHotKeysMap(_prefs.CustomizedMenuItems);
            EditorApplication.update += KeyboardInputUpdate;
        }

        private void UpdateHotKeysMap(List<MenuItemLink> menuItemLinks)
        {
            _hotKeysMap = new Dictionary<string, string>(10);

            foreach (var item in menuItemLinks)
            {
                foreach (var hotKey in item.CustomHotKeys)
                {
                    if (!hotKey.IsVerified) continue;
                    _hotKeysMap[hotKey] = item.Path;
                }
            }
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
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="hotkey"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        private bool IsValidHotKey(MenuItemHotKey hotkey, out string error)
        {
            error = string.Empty;

            var checkModifiers = hotkey.Alt | hotkey.Cmd | hotkey.Shift;
            if (!checkModifiers)
            {
                error = " there needs to have at least one modifier alt or cmd or shift!";
                return false;
            }

            var key = hotkey.Key.ToCharArray();

            if (!(key.Length == 1 && (((key[0] - 42) >= 0 && (key[0] - 42) <= 9)) ||
                  (key[0] >= 'A' && key[0] <= 'Z') ||
                  (key[0] >= 'a' && key[0] <= 'z')))
            {
                error = "please use this interval of available symbols for the key A-Z, a-z, 0-9";
                return false;
            }

            UpdateHotKeysMap(_menuItems);
            string itemPath;
            _hotKeysMap.TryGetValue(hotkey, out itemPath);

            if (itemPath == null)
            {
                var item = _menuItems.Find(itemLink => hotkey.Equals(itemLink.HotKey));
                if (item != null)
                {
                    itemPath = item.Path;
                }
            }

            if (itemPath != null)
            {
                error = itemPath + " this menuitem already contains hotkey " + hotkey;
                return false;
            }

            return true;
        }

        private bool TryAddHotKeyToItem(MenuItemLink menuItem, MenuItemHotKey hotkey, out string error)
        {
            if (!IsValidHotKey(hotkey, out error)) return false;

            hotkey.IsVerified = true;
            menuItem.UpdateLabel();
            UpdateHotKeysMap(_menuItems);
            return true;
        }

        private void CustomHotKeysEditable_OnRemoved(ReorderableList list)
        {
            list.list.RemoveAt(list.index);
            if (list.index >= list.list.Count - 1)
                list.index = list.list.Count - 1;
            
            UpdateHotKeysMap(_menuItems);
        }
    }
}