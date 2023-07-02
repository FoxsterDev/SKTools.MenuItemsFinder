using SKTools.Core.Editor;
using UnityEditor;

namespace SKTools.MenuItemsFinder
{
    internal partial class MenuItemsFinder
    {
#if !FOXSTER_DEV_MODE
        private const string MenuAssetPath = "SKTools/";//"Assets/SKTools/";
#else
        private const string MenuAssetPath = "SKTools/";
#endif

        private const int Priority = 2000;

        private Surrogate<IGUIContainer, Assets> _target;

        [MenuItem(MenuAssetPath + "MenuItems Finder #%m", false, Priority + 2)]
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
            var container = CustomEditorWindow<Configuration>.GetWindow(createIfNotExist);
            if (container == null)
            {
                return;
            }

            Utility.DiagnosticRun(LoadMenuItems);

            var assetsDirectory = Utility.GetPathRelativeToCurrentDirectory("Editor Resources");
            var assets = new Assets(assetsDirectory);

            Utility.DiagnosticRun(assets.Load);

            container.DrawGuiCallback = OnWindowGui;
            container.CloseCallback = OnWindowClosed;
            container.LostFocusCallback = OnWindowLostFocus;
            container.FocusCallback = OnWindowFocus;

            _target = new Surrogate<IGUIContainer, Assets>(container, assets);

            if (createIfNotExist)
            {
                container.Show();
                container.Focus();
            }
        }

        private void OnWindowFocus(IGUIContainer window)
        {
            _prefs.PreviousFilterString = _prefs.FilterString;
            SetFilteredItems(_prefs.FilterString);
            window.Repaint();
        }

        private void OnWindowGui(IGUIContainer window)
        {
            if (!_isLoaded || EditorApplication.isCompiling)
            {
                DrawUnavailableState(window.Position);
                return;
            }

            DrawSearchBar();
            DrawMenuBar();
            DrawItems();
            DrawSupportBar();
        }

        private void OnWindowLostFocus(IGUIContainer window)
        {
            SavePrefs();
        }

        private void OnWindowClosed(IGUIContainer window)
        {
            SavePrefs();
        }
    }
}