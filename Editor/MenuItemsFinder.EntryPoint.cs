using System.IO;
using SKTools.Module.Base;
using UnityEditor;

namespace SKTools.MenuItemsFinder
{
    internal partial class MenuItemsFinder
    {
        private Surrogate<MenuItemsFinderEditorWindow, MenuItemsFinderAssetsProvider> _targetGui;

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
            _targetGui = new Surrogate<MenuItemsFinderEditorWindow, MenuItemsFinderAssetsProvider>(createIfNotExist, assetsDirectory);
            if (_targetGui.Container == null)
            {
                return;
            }

            _targetGui.Container.DrawGuiCallback = OnWindowGui;
            _targetGui.Container.CloseCallback = OnWindowClosed;
            _targetGui.Container.LostFocusCallback = OnWindowLostFocus;

            if (createIfNotExist)
            {
                _targetGui.Container.Show();
            }
        }
    }
}