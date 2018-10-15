using UnityEditor;
using UnityEngine;

namespace SKTools.MenuItemsFinder
{
    internal partial class MenuItemsFinder
    {
        private MenuItemsFinderWindow _window;

        private static bool IsWindowOpen
        {
            get { return EditorPrefs.GetBool(typeof(MenuItemsFinder).Name, false); }
            set { EditorPrefs.SetBool(typeof(MenuItemsFinder).Name, value); }
        }

        [MenuItem("SKTools/MenuItems Finder #%m")]
        private static void ShowWindow()
        {
            var window = GetWindow(true);
            var finder = GetFinder();
            finder.SetWindow(window);

            window.Show();
            window.Focus();
            IsWindowOpen = true;
        }

        [InitializeOnLoadMethod]
        private static void MenuItemsFinderWindow_Reload()
        {
            if (!IsWindowOpen) return;
            
            var window = GetWindow(false);
            if (window == null)
            {
                IsWindowOpen = false;
                return;
            }

            var finder = GetFinder();
            finder.SetWindow(window);
            window.Focus();
        }

        private static MenuItemsFinderWindow GetWindow(bool createIfNotExist)
        {
            Debug.Log("MenuItemsFinderWindow GetWindow");

            var objectsOfTypeAll = Resources.FindObjectsOfTypeAll(typeof(MenuItemsFinderWindow));
            if (objectsOfTypeAll.Length < 1)
            {
                if (!createIfNotExist) return null;
                return ScriptableObject.CreateInstance<MenuItemsFinderWindow>();
            }

            var window = (MenuItemsFinderWindow) objectsOfTypeAll[0];
            return window;
        }

        private void SetWindow(MenuItemsFinderWindow window)
        {
            _window = window;
            LoadAssets();
            window.autoRepaintOnSceneChange = true;
            window.titleContent = new GUIContent(typeof(MenuItemsFinderWindow).Name);
            window.DrawGuiCallback = OnWindowGui;
            window.CloseCallback = OnWindowClosed;
            window.LostFocusCallback = OnWindowLostFocus;
        }

        private void OnWindowLostFocus()
        {
            SavePrefs();
        }

        private void OnWindowClosed()
        {
            IsWindowOpen = false;
            SavePrefs();
            Dispose();
        }

        private void OnWindowGui()
        {
            if (IsCompiling())
            {
                return;
            }

            if (!_isCreatedStyles)
            {
                _isCreatedStyles = true;
                CreateStyles();
            }
            
            DrawSearchBar();
            DrawMenuBar();
            DrawItems();

            CleanRemovedItems();
        }
    }
}