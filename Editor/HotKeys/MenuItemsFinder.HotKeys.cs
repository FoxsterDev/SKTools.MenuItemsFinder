namespace SKTools.MenuItemsFinder
{
    internal partial class MenuItemsFinder
    {
        private bool TryAddHotKeyToItem(MenuItemLink menuItem, MenuItemHotKey hotkey, out string error)
        {
            if (!IsValidHotKey(hotkey, out error))
            {
                return false;
            }

            if (menuItem.CustomHotKeys.Count < 1)
            {
                menuItem.CustomHotKeys.Add(hotkey);
            }
            else
            {
                menuItem.CustomHotKeys[0] = hotkey;
            }

            hotkey.IsVerified = true;
            UpdateLabel(menuItem);
            UpdateHotKeysMap(_menuItems);
            return true;
        }

        /// <summary>
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

            var key = hotkey.Key;

            if (!(key.Length == 1 && key[0] - 48 >= 0 && key[0] - 48 <= 9 ||
                  key[0] >= 'A' && key[0] <= 'Z' ||
                  key[0] >= 'a' && key[0] <= 'z'))
            {
                error = "please use this interval of available symbols for the key A-Z, a-z, 0-9";
                return false;
            }

            UpdateHotKeysMap(_menuItems);

            string itemPath;
            _hotKeysMap.TryGetValue(hotkey, out itemPath);

            if (itemPath == null)
            {
                var item = _menuItems.Find(itemLink => hotkey.Equals(itemLink.OriginalHotKey));

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
    }
}
