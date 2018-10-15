using UnityEditor;

namespace SKTools.MenuItemsFinder
{
    internal class MenuItemsFinderWindow : EditorWindow
    {
        public  FinderDelegate DrawGuiCallback;
        public  FinderDelegate LostFocusCallback;
        public  FinderDelegate CloseCallback;

        private void OnGUI()
        {
            if (DrawGuiCallback != null) DrawGuiCallback();
        }

        private void OnSelectionChange()
        {
            Repaint();
        }

        private void OnLostFocus()
        {
            if (LostFocusCallback != null) LostFocusCallback();
        }
    
        private void OnDestroy()
        {
            if (CloseCallback != null)  CloseCallback();
            
            DrawGuiCallback = null;
            LostFocusCallback = null;
            CloseCallback = null;
        }
    }
}