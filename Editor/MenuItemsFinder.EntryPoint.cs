using UnityEditor;
using SKTools.Base.Editor;

namespace SKTools.MenuItemsFinder
{
    internal partial class MenuItemsFinder
    {
        private Surrogate<MenuItemsFinderEditorWindow, MenuItemsFinderAssetsContainer> _targetGui;

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
            var container = CustomEditorWindow<MenuItemsFinderEditorWindow>.GetWindow(createIfNotExist);
            
            if (container != null)
            {
                Utility.DiagnosticRun(LoadMenuItems);
                
                var assetsDirectory =  Utility.GetPath("Editor Resources");
                var assets = new MenuItemsFinderAssetsContainer(assetsDirectory);

                Utility.DiagnosticRun(assets.Load);
                
                container.DrawGuiCallback = OnWindowGui;
                container.CloseCallback = OnWindowClosed;
                container.LostFocusCallback = OnWindowLostFocus;

                _targetGui = new Surrogate<MenuItemsFinderEditorWindow, MenuItemsFinderAssetsContainer>(container, assets);

                if (createIfNotExist)
                {
                    container.Show();
                }
            }
        }
    }
}