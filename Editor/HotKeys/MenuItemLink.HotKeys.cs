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
        [NonSerialized] public MenuItemHotKey OriginalHotKey;
        [NonSerialized] public MenuItemHotKey EditHotKey = new MenuItemHotKey();
        [NonSerialized] public string EditHotKeySymbol = "";

        public bool HasCustomHotKey
        {
            get { return CustomHotKeys != null && CustomHotKeys.Count > 0; }
        }
        
        public void UpdateOriginalHotKey()
        {
            var startIndex = -1;
            OriginalHotKey = MenuItemHotKey.Create(OriginalPath, out startIndex);
     
            if (startIndex > -1)
            {
                OriginalName = OriginalPath.Substring(0, startIndex - 1);
            }
        }
        
        public void UpdateCustomHotKeysFrom(MenuItemLink item)
        {
            if (item.CustomHotKeys != null)
            {
                CustomHotKeys = item.CustomHotKeys;
            }
        }

        public void UpdateLabelWithHotKey()
        {
            var hotKey = OriginalHotKey ?? (CustomHotKeys.Count > 0 ? CustomHotKeys[0] : null);
            if (hotKey != null)
            {
                Label = string.Concat(Label, " (<b><color=cyan>", hotKey.Formatted, "</color></b>)");
            }
        }
    }
}