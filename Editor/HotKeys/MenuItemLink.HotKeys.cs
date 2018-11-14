using System;
using System.Collections.Generic;
using UnityEngine;

namespace SKTools.MenuItemsFinder
{
    internal partial class MenuItemLink
    {
        public List<MenuItemHotKey> CustomHotKeys = new List<MenuItemHotKey>(1);

        /// <summary>
        /// Original hotkey from menuitem path
        /// </summary>
        [NonSerialized]
        public MenuItemHotKey OriginalHotKey;

        [NonSerialized]
        public MenuItemHotKey EditHotKey = new MenuItemHotKey();

        [NonSerialized]
        public string EditHotKeySymbol = string.Empty;

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

        public void UpdateLabelWithHotKey(Color32 color)
        {
            var hotKey = OriginalHotKey ?? (CustomHotKeys.Count > 0
                                                ? CustomHotKeys[0]
                                                : null);
            if (hotKey != null)
            {
                var hexColor = string.Concat(color.r.ToString("X2"), color.g.ToString("X2"), color.b.ToString("X2"));
                Label = string.Concat(
                    Label, string.Format(" (<b><color=#{0}>", hexColor), hotKey.Formatted,
                    "</color></b>)");
            }
        }
    }
}
