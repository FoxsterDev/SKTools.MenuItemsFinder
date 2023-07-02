using SKTools.Editor.GuiElementsSystem;
using UnityEngine;

namespace SKTools.MenuItemsFinder
{
    internal partial class MenuItemsFinder
    {
        private readonly GUILayoutOption[] _options = { GUILayout.MinWidth(200) };
        private GuiLayoutSearchBar _searchBar;

        private string DrawSearchBar(string text, bool focusControl = true)
        {
            return (_searchBar ?? (_searchBar = new GuiLayoutSearchBar("SearchTextField", _options)))
                .Draw(text, focusControl);
        }

        private void DrawSearchBar()
        {
            FilterMenuItems = DrawSearchBar(FilterMenuItems, _itemFocusControl == null);
        }
    }
}