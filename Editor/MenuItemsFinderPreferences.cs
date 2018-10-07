using System.Collections.Generic;

namespace SKTools.MenuItemsFinder
{
    [System.Serializable]
    internal class MenuItemsFinderPreferences
    {
        [System.NonSerialized] public string PreviousFilterString = null;

        public string FilterString = string.Empty;
        public bool OnlyWithValidate;
        public List<MenuItemLink> CustomizedMenuItems = new List<MenuItemLink>();
    }
}