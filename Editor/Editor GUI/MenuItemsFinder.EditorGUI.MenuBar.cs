using SKTools.Editor.GuiElementsSystem;
using UnityEditor;

namespace SKTools.MenuItemsFinder
{
    internal partial class MenuItemsFinder
    {
        private IGuiElement _menuBar;

        private void DrawMenuBar()
        {
            _prefs.ShowMenuBar = EditorGUILayout.Foldout(_prefs.ShowMenuBar, "Menu");
            if (_prefs.ShowMenuBar)
            {
                if (_menuBar == null)
                {
                    _menuBar = new GuiLayoutHorizontal(
                        new GuiListElements(
                            GuiElementsFactory.CreateLayoutToggleWithFixedWidth(
                                "Hide Unity Items",
                                v => { _prefs.HideUnityItems = v; }, () => _prefs.HideUnityItems, 120),
                            GuiElementsFactory.CreateLayoutToggleWithFixedWidth(
                                "Show Only Starred",
                                v => { _prefs.ShowOnlyStarred = v; }, () => _prefs.ShowOnlyStarred, 120),
                            GuiElementsFactory.CreateLayoutToggleWithFixedWidth(
                                "Hide Missed Items",
                                v => { _prefs.HideAllMissed = v; }, () => _prefs.HideAllMissed, 120)
                        ));
                }

                _menuBar.Draw();
            }
        }
    }
}