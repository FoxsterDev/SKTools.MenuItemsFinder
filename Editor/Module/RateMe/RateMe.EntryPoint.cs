using SKTools.Base.Editor;
using UnityEditor;
using UnityEngine;

namespace SKTools.RateMeWindow
{
    internal partial class RateMe
    {
        private Surrogate<IGUIContainer, Assets> _targetGui;
        private RateMeConfig _config;
        
        private static RateMe _instance;

        private static RateMe GetRateMe()
        {
            return _instance ?? (_instance = new RateMe());
        }

        private RateMeConfig Config
        {
            get { return _config ?? (_config = new RateMeConfig().Load()); }
        }
        
        [MenuItem("SKTools/Rate Me Test")]
        private static void ShowWindow()
        {
            GetRateMe().SetUpWindow(true);
        }
      
        [InitializeOnLoadMethod]
        private static void MenuItemsFinderWindow_CheckReload()
        {
            GetRateMe().SetUpWindow(false);
        }

        private void SetUpWindow(bool createIfNotExist)
        {
            var container = CustomEditorWindow<Window>.GetWindow(createIfNotExist);
            if (container == null) return;
            
            _feedbackMessage = Config.FeedbackMessage;
            _starRects = new Rect[Config.MaxStar];
            
            var assetsDirectory = Utility.GetPath("Editor Resources");
            var assets = new Assets(assetsDirectory);

            Utility.DiagnosticRun(assets.Load);
                
            _targetGui = new Surrogate<IGUIContainer, Assets>(container, assets);
            _targetGui.Container.DrawGuiCallback = DrawRateGui;

            if (createIfNotExist)
            {
                _targetGui.Container.Show();
            }

           
            SetCountStar(1);
        }
    }
}