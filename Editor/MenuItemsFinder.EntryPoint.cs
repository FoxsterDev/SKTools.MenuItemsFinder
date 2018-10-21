using System.IO;
using SKTools.Module.Base;
using UnityEditor;

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
            var assetsDirectory = Path.Combine(GetDirectory(GetType()), "Editor Resources");
            _window = new Surrogate<MenuItemsFinderEditorWindow, AssetsProvider>(createIfNotExist, assetsDirectory);
            if (_window.Container == null)
            {
                return;
            }

            _window.Container.DrawGuiCallback = OnWindowGui;
            _window.Container.CloseCallback = OnWindowClosed;
            _window.Container.LostFocusCallback = OnWindowLostFocus;
        }
    }
}