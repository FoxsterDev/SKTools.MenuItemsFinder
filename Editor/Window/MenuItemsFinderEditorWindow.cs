using System.Diagnostics;
using SKTools.Module.Base;
using UnityEngine;

namespace SKTools.MenuItemsFinder
{
    /// <summary>
    /// This window will show editable menuitems
    /// </summary>
    internal class MenuItemsFinderEditorWindow : SKEditorWindow<MenuItemsFinderEditorWindow>, GUIContainerInterface
    {
        protected override GUIContent GetTitleContent
        {
            get { return new GUIContent("MenuItems"); }
        }

        /*protected override StackTrace GetStackTrace()
        {
            return new StackTrace(true);
        }*/

        protected override Vector2? GetMinSize
        {
            get { return new Vector2(350, 450); }
        }

        protected override bool GetAutoRepaintOnSceneChange
        {
            get { return true; }
        }

        protected override bool GetAutoRepaintOnSelectionChange
        {
            get { return true; }
        }
    }
}