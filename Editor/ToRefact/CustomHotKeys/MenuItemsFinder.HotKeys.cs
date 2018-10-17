using UnityEditorInternal;

namespace SKTools.MenuItemsFinder
{
    internal partial class MenuItemsFinder
    {
        private ReorderableList _selectedMenuItemCustomHotKeysEditable;

        
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
    }
}