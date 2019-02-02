using SKTools.Core.Editor;
using UnityEngine;

namespace SKTools.MenuItemsFinder
{
    /// <summary>
    /// This window will show editable menuitems
    /// </summary>
    internal class Configuration : CustomEditorWindow<Configuration>, IGUIContainer
    {
        protected override GUIContent GetTitleContent
        {
            get { return new GUIContent("MenuItems"); }
        }

        protected override Vector2? GetMinSize
        {
            get { return new Vector2(600, 450); }
        }

        protected override Rect? GetDefaultPosition
        {
            get { return new Rect(Screen.width / 2 - 600 / 2, Screen.height / 2 - 450 / 2, 600, 450); }
        }

        protected override bool GetAutoRepaintOnSceneChange
        {
            get { return true; }
        }

        protected override bool GetAutoRepaintOnSelectionChange
        {
            get { return true; }
        }

        public Rect Position
        {
            get { return position; }
        }
    }
}
