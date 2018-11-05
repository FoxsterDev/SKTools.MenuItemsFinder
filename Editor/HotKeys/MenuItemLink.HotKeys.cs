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
        [NonSerialized] public MenuItemHotKey EditHotKey = new MenuItemHotKey();
        
        public void UpdateOriginalHotKey()
        {
            var startIndex = -1;
            HotKey = MenuItemHotKey.Create(OriginalPath, out startIndex);
     
            if (startIndex > -1)
            {
                OriginalName = OriginalPath.Substring(0, startIndex - 1);
            }
        }
        
        public void UpdateCustomHotKeysFrom(MenuItemLink item)
        {
            CustomHotKeys = item.CustomHotKeys;
        }

        public void UpdateLabelWithHotKey()
        {
            var hotKey = HotKey ?? (CustomHotKeys.Count > 0 ? CustomHotKeys[0] : null);
            if (hotKey != null)
            {
                Label = string.Concat(Label, " (<b><color=cyan>", hotKey.Formatted, "</color></b>)");
            }
        }
    }
}