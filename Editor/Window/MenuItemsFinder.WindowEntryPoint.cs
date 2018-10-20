using UnityEditor;
using UnityEngine;

namespace SKTools.MenuItemsFinder
{
    internal partial class MenuItemsFinder
    {
        private static bool IsWindowOpen
        {
            get { return EditorPrefs.GetBool(typeof(MenuItemsFinder).Name, false); }
            set { EditorPrefs.SetBool(typeof(MenuItemsFinder).Name, value); }
        }

        [MenuItem("SKTools/MenuItems Finder #%m")]
        private static void ShowWindow()
        {
            var window = GetWindow<MenuItemsFinderEditorWindow>(true);

            GetFinder().SetUpWindow(window);
            
            window.Show();
            IsWindowOpen = true;
        }

        [InitializeOnLoadMethod]
        private static void MenuItemsFinderWindow_CheckReload()
        {
            if (!IsWindowOpen) return;
            
            var window = GetWindow<MenuItemsFinderEditorWindow>(false);
            if (window == null)
            {
                IsWindowOpen = false;
                return;
            }

            GetFinder().SetUpWindow(window);
        }

        private void SetUpWindow(MenuItemsFinderEditorWindow window)
        {
            LoadMenuItems();
            LoadGuiAssets();
            
            window.autoRepaintOnSceneChange = true;
            window.titleContent = new GUIContent("MenuItems");
            window.minSize = new Vector2(350, 450);
            
            window.DrawGuiCallback = OnWindowGui;
            window.CloseCallback = OnWindowClosed;
            window.LostFocusCallback = OnWindowLostFocus;
            
            window.Focus();
        }

        private void OnWindowLostFocus(MenuItemsFinderEditorWindow window)
        {
            SavePrefs();
        }

        private void OnWindowClosed(MenuItemsFinderEditorWindow window)
        {
            IsWindowOpen = false;
            SavePrefs();
        }

        private void OnWindowGui(MenuItemsFinderEditorWindow window)
        {
            if (!_isLoaded || EditorApplication.isCompiling)
            {
                DrawUnvailableState(window);
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