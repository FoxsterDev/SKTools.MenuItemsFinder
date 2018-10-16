using UnityEditor;

namespace SKTools.MenuItemsFinder
{
    /// <summary>
    /// This window will show with editable menuitems
    /// </summary>
    internal class MenuItemsFinderWindow : EditorWindow
    {
        public  FinderDelegate<MenuItemsFinderWindow> DrawGuiCallback;
        public  FinderDelegate<MenuItemsFinderWindow> LostFocusCallback;
        public  FinderDelegate<MenuItemsFinderWindow> CloseCallback;

        /// <summary>
        /// we can easily switch gui content of this window
        /// </summary>
        private void OnGUI()
        {
            if (DrawGuiCallback != null) DrawGuiCallback(this);
        }

        /// <summary>
        /// Some menuitems requeres validating  method, and it need to update visual state of items
        /// </summary>
        private void OnSelectionChange()
        {
            Repaint();
        }
        
        /// <summary>
        /// This callbacks are used for saving prefs
        /// </summary>
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