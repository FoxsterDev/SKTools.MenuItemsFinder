using UnityEditor;
using UnityEngine;

namespace SKTools.MenuItemsFinder
{
    internal partial class MenuItemsFinder
    {
        [MenuItem("SKTools/MenuItems Finder #%m")]
        private static void ShowWindow()
        {
            var window = MenuItemsFinderEditorWindow.GetWindow(true);
            GetFinder().SetUpWindow(window);
            window.Show();
        }

        [InitializeOnLoadMethod]
        private static void MenuItemsFinderWindow_CheckReload()
        {
            var window = MenuItemsFinderEditorWindow.GetWindow();
            if (window == null)
            {
                return;
            }
            GetFinder().SetUpWindow(window);
        }

        private void SetUpWindow(MenuItemsFinderEditorWindow window)
        {
            LoadMenuItems();
            LoadGuiAssets();
            
            window.DrawGuiCallback = OnWindowGui;
            window.CloseCallback = OnWindowClosed;
            window.LostFocusCallback = OnWindowLostFocus;
        }

        private void OnWindowLostFocus(Rect position)
        {
            SavePrefs();
        }

        private void OnWindowClosed(Rect position)
        {
            SavePrefs();
        }

        private void OnWindowGui(Rect position)
        {
            if (!_isLoaded || EditorApplication.isCompiling)
            {
                DrawUnvailableState(position);
                return;
            }
          
            if (!_isLoadedWindowStyles)
            {
                _isLoadedWindowStyles = true;
                LoadWindowStyles();
            }
            
            DrawSearchBar();
            DrawMenuBar();
            DrawItems();
            DrawSupportBar();
        }
    }
}