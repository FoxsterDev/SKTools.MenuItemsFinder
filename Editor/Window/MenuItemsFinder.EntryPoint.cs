using SKTools.Module.Base;
using UnityEditor;
using UnityEngine;

namespace SKTools.MenuItemsFinder
{
    internal partial class MenuItemsFinder
    {
        private Surrogate<MenuItemsFinderEditorWindow, AssetsProvider> _window;

        [MenuItem("SKTools/MenuItems Finder #%m")]
        private static void ShowWindow()
        {
            GetFinder().SetUpWindow(true);
        }

        [InitializeOnLoadMethod]
        private static void MenuItemsFinderWindow_CheckReload()
        {
            GetFinder().SetUpWindow(false);
        }

        private void SetUpWindow(bool createIfNotExist)
        {
            _window = new Surrogate<MenuItemsFinderEditorWindow, AssetsProvider>(createIfNotExist);
            if (_window == null)
            {
                return;
            }

            _window.Container.DrawGuiCallback = OnWindowGui;
            _window.Container.CloseCallback = OnWindowClosed;
            _window.Container.LostFocusCallback = OnWindowLostFocus;
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