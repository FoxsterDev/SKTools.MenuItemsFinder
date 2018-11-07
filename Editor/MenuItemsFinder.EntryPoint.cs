using UnityEditor;
using SKTools.Base.Editor;
using SKTools.Base.Editor.GuiElementsSystem;
using UnityEngine;

namespace SKTools.MenuItemsFinder
{
    internal partial class MenuItemsFinder
    {
        private Surrogate<IGUIContainer, Assets> _target;

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