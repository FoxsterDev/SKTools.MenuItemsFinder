using UnityEngine;

namespace SKTools.MenuItemsFinder
{
    /// <summary>
    /// This window will show editable menuitems
    /// </summary>
    internal class MenuItemsFinderEditorWindow : SKEditorWindow<MenuItemsFinderEditorWindow>, SKGUIContainerInterface
    {
        protected override GUIContent TitleContent
        {
            get { return new GUIContent("MenuItems"); }
        }

        protected override Vector2? MinSize
        {
            get { return new Vector2(350, 450); }
        }

        protected override bool AutoRepaintOnSceneChange
        {
            get { return true; }
        }
    }
}