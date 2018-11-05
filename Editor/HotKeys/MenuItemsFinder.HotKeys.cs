using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace SKTools.MenuItemsFinder
{
    internal partial class MenuItemsFinder
    {
       /* private ReorderableList _selectedMenuItemCustomHotKeysEditable;

        private void DrawMenuItemHotKeys()
        {
            _selectedMenuItemCustomHotKeysEditable.DoLayoutList();
        }
        
        private void CustomHotKeysEditable()
        {
            _selectedMenuItemCustomHotKeysEditable = new ReorderableList(_selectedMenuItem.CustomHotKeys, typeof(MenuItemHotKey), true, true, true, true);
            _selectedMenuItemCustomHotKeysEditable.drawHeaderCallback += (rect) =>
            {
                GUI.Label(rect, "HotKeys " + _selectedMenuItem.HotKey);
            };
            _selectedMenuItemCustomHotKeysEditable.onReorderCallback += CustomHotKeysEditable_OnReorder; 
            _selectedMenuItemCustomHotKeysEditable.drawElementCallback += CustomHotKeysEditable_DrawHotKey;
            _selectedMenuItemCustomHotKeysEditable.onRemoveCallback += CustomHotKeysEditable_OnRemoved;
        }

        private void CustomHotKeysEditable_OnReorder(ReorderableList list)
        {
            UpdateLabel(_selectedMenuItem);
        }*/

        private bool TryAddHotKeyToItem(MenuItemLink menuItem, MenuItemHotKey hotkey, out string error)
        {
            if (!IsValidHotKey(hotkey, out error)) return false;

            hotkey.IsVerified = true;
            UpdateLabel(menuItem);
            UpdateHotKeysMap(_menuItems);
            return true;
        }

        /*private void CustomHotKeysEditable_OnRemoved(ReorderableList list)
        {
            list.list.RemoveAt(list.index);
            if (list.index >= list.list.Count - 1)
                list.index = list.list.Count - 1;

            UpdateLabel(_selectedMenuItem);
            UpdateHotKeysMap(_menuItems);
        }*/
        
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
                    itemPath = item.OriginalPath;
                }
            }

            if (itemPath != null)
            {
                error = itemPath + " this menuitem already contains hotkey " + hotkey;
                return false;
            }

            return true;
        }
        
        
        /*private void CustomHotKeysEditable_DrawHotKey(Rect rect, int index, bool isactive, bool isfocused)
        {
            var hotkey = (MenuItemHotKey)  _selectedMenuItemCustomHotKeysEditable.list[index];
            
            if (!hotkey.IsVerified && !hotkey.IsOriginal)
            {
                var allWidth = rect.width;
                var width = allWidth * 0.2f;

                rect.width = width;
                if (GUI.Button(rect, "Check&Add"))
                {
                    var error = "";
                    if (!TryAddHotKeyToItem(_selectedMenuItem, hotkey, out error))
                    {
                        EditorUtility.DisplayDialog("Something went wrong!", error, "Try again!");
                    }
                }
                
                rect.x += width;
                
                width = allWidth * 0.15f;
                rect.width = width;
                
                hotkey.Key = GUI.TextField(rect, hotkey.Key);
                rect.x += width;
                
                GUI.Label(rect, " Key");
                rect.x += width;
                
                hotkey.Alt = GUI.Toggle(rect, hotkey.Alt, " Alt");
                rect.x += width;
                
                hotkey.Shift = GUI.Toggle(rect, hotkey.Shift, " Shift");
                rect.x += width;
                
                hotkey.Cmd = GUI.Toggle(rect, hotkey.Cmd, " Cmd");
            }
            else
            {
                GUI.Label(rect, hotkey.Formatted);
            }
        }*/
    }
}