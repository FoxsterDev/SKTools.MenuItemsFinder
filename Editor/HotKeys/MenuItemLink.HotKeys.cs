using System;
using System.Collections.Generic;

namespace SKTools.MenuItemsFinder
{
    internal partial class MenuItemLink
    {
        public List<MenuItemHotKey> CustomHotKeys;
        [NonSerialized] public MenuItemHotKey HotKey;
   
        partial void HasCustomHotKeys(ref bool hasHotKeys)
        {
            hasHotKeys = CustomHotKeys != null && CustomHotKeys.Count > 0;
        }
       
        partial void UpdateHotKeys(MenuItemLink item)
        {
            if (CustomHotKeys == null)
            {
                CustomHotKeys = new List<MenuItemHotKey>(1);
                
                HotKey = new MenuItemHotKey();
                var startIndex = -1;
                var hotkeyString = "";
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

            if (item != null)
            {
                CustomHotKeys = item.CustomHotKeys;
            }

            var hasHotKey = HotKey ?? (CustomHotKeys.Count > 0 ? CustomHotKeys[0] : null);
            if (hasHotKey != null)
            {
                Label = string.Concat(Label, " <color=cyan>", hasHotKey.Formatted, "</color>");
            }
        }
    }
}