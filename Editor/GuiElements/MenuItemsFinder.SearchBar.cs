using SKTools.Base.Editor.GuiElementsSystem;
using UnityEngine;

namespace SKTools.MenuItemsFinder
{
    internal partial class MenuItemsFinder
    {
        private GuiLayoutSearchBar _searchBar;
        private GUILayoutOption[] _options = { GUILayout.MinWidth(200) };
        
        private string DrawSearchBar(string text, bool focusControl = true)
        {
            return (_searchBar ?? (_searchBar = new GuiLayoutSearchBar("SearchTextField", _options)))
                .Draw(text, focusControl);
        }
    }
}