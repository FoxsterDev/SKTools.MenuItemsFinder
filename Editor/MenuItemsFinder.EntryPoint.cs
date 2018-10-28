using UnityEditor;
using SKTools.Base.Editor;

namespace SKTools.MenuItemsFinder
{
    internal partial class MenuItemsFinder
    {
        private Surrogate<IGUIContainer, Assets> _targetGui;

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
            var container = CustomEditorWindow<Window>.GetWindow(createIfNotExist);
            if (container == null) return;

            Utility.DiagnosticRun(LoadMenuItems);
                
            var assetsDirectory = Utility.GetPathRelativeToExecutableCurrentFile("Editor Resources");
            var assets = new Assets(assetsDirectory);

            Utility.DiagnosticRun(assets.Load);
   
            container.DrawGuiCallback = OnWindowGui;
            container.CloseCallback = OnWindowClosed;
            container.LostFocusCallback = OnWindowLostFocus;

            _targetGui = new Surrogate<IGUIContainer, Assets>(container, assets);

            if (createIfNotExist)
            {
                container.Show();
            }
        }
    }
}