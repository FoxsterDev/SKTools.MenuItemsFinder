using System.Collections.Generic;
using UnityEngine.Serialization;

namespace SKTools.MenuItemsFinder
{
    [System.Serializable]
    internal class MenuItemsFinderPreferences
    {
        [System.NonSerialized]
        public string PreviousSearchString = null;
        [FormerlySerializedAs("SearchString")] public string FilterString = string.Empty;
        public bool OnlyWithValidate = false;
        public List<string> StarredMenuItems = new List<string>();
    }
}