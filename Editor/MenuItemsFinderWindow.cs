using System;
using UnityEditor;

namespace SKTools.MenuItemsFinder
{
    internal class MenuItemsFinderWindow : EditorWindow
    {
        public  FinderDelegate<MenuItemsFinderWindow> DrawGuiCallback;
        public  FinderDelegate<MenuItemsFinderWindow> LostFocusCallback;
        public  FinderDelegate<MenuItemsFinderWindow> CloseCallback;

        private void OnGUI()
        {
            if (DrawGuiCallback != null) DrawGuiCallback(this);
        }

        private void OnSelectionChange()
        {
            Repaint();
        }

        private void OnLostFocus()
        {
            if (LostFocusCallback != null) LostFocusCallback(this);
        }
    
        private void OnDestroy()
        {
            if (CloseCallback != null)  CloseCallback(this);
            
            DrawGuiCallback = null;
            LostFocusCallback = null;
            CloseCallback = null;
        }
    }
}