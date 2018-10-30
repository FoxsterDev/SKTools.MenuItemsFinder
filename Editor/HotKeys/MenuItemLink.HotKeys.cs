using System;
using System.Collections.Generic;

namespace SKTools.MenuItemsFinder
{
    internal partial class MenuItemLink
    {
        public List<MenuItemHotKey> CustomHotKeys = new List<MenuItemHotKey>(1);
        /// <summary>
        /// Original hotkey from menuitem path
        /// </summary>
        [NonSerialized] public MenuItemHotKey HotKey;

        public void UpdateOriginalHotKey()
        {
            HotKey = new MenuItemHotKey();
            var startIndex = -1;
            var hotkeyString = string.Empty;
            MenuItemHotKey.ExtractFrom(Path, out startIndex, out hotkeyString, out HotKey.Key, out HotKey.Shift,
                out HotKey.Alt, out HotKey.Cmd);

            if (startIndex > -1)
            {
                HotKey.IsOriginal = true;
                HotKey.IsVerified = true;
                Path = Path.Substring(0, startIndex - 1);
            }
            else
            {
                HotKey = null;
            }
        }
        
        public void UpdateCustomHotKeysFrom(MenuItemLink item)
        {
            CustomHotKeys = item.CustomHotKeys;
        }

        private string AddHotKey(string str, MenuItemHotKey hotKey)
        {
            if (hotKey == null) return str;
            str = string.Concat(str, ", (", hotKey.Formatted, ")");
            return str;
        }
        
        public void UpdateLabelWithHotKeys()
        {
            var hotkeysInfo = string.Empty;
                hotkeysInfo = AddHotKey(hotkeysInfo, HotKey);
            
            foreach (var hotKey in CustomHotKeys)
            {
                hotkeysInfo = AddHotKey(hotkeysInfo, hotKey);
            }
           
            if (!string.IsNullOrEmpty(hotkeysInfo))
            {
                Label = string.Concat(Label, " <b><color=cyan>", hotkeysInfo, "</color></b>");
            }
        }
    }
}