using System.Text;
using SKTools.Base.Editor;
using UnityEditor;
using UnityEngine;

namespace SKTools.RateMeWindow
{
    internal partial class RateMe
    {
        public const int MaxStar = 5;
        public const int SizeStar = 64;
        public const int MinStar = 3;
        
        private bool _isFeedback;
        private string _message = "Please Enter your message here";
        private int _countStar;
        private readonly Rect[] _starRects = new Rect[MaxStar];
        private string _assetStoreUrl;


        
        private Surrogate<IGUIContainer, Assets> _targetGui;

        private static RateMe _instance;

        private static RateMe GetRateMe()
        {
            return _instance ?? (_instance = new RateMe());
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
            
            if (container != null)
            {
                var assetsDirectory = Utility.GetPath("Editor Resources");
                var assets = new Assets(assetsDirectory);

                SKTools.Base.Editor.Utility.DiagnosticRun(assets.Load);
                
                container.DrawGuiCallback = OnWindowGui;
              
                _targetGui = new Surrogate<IGUIContainer, Assets>(container, assets);

                if (createIfNotExist)
                {
                    container.Show();
                }
            }
        }

        private void OnWindowGui(IGUIContainer window)
        {
            var position = window.Position;
            
            if (_isFeedback)
            {
                GUI.Label(new Rect(5, 5, position.width, 24), "FEEDBACK/SUGGESTION");
                _message = GUI.TextArea(new Rect(5, 24, position.width - 10, position.height - 128), _message);

                if (GUI.Button(new Rect(position.width / 2 - 156, position.height - 64, 156, 64), "Back",
                    _targetGui.Assets.ButtonStyle))
                    _isFeedback = false;

                if (GUI.Button(new Rect(position.width / 2 + 5, position.height - 64, 156, 64), "Send Email",
                    _targetGui.Assets.ButtonStyle))
                    EmailMe();

                return;
            }

            var height = (position.height - SizeStar) / 2;
            var rectUpper = new Rect(0, 0, position.width, height);

            GUI.Label(rectUpper, "How do you like it?",  _targetGui.Assets.LabelStyle);

            var spaceX = (position.width / MaxStar - SizeStar) / 2;
            var rect = new Rect(0, height, SizeStar, SizeStar);

            for (var i = 0; i < MaxStar; i++)
            {
                rect.x += spaceX;
                _starRects[i] = rect;
                if (GUI.Button(_starRects[i], string.Empty,  _targetGui.Assets.StarStyles[i]))
                {
                    _countStar = i + 1;
                    SetCountStar(_countStar);
                    return;
                }

                rect.x += SizeStar + spaceX;
            }

            if (GUI.Button(new Rect((position.width - 128) / 2, position.height - 64, 128, 64), "Rate",
                _targetGui.Assets.ButtonStyle))
            {
                if (_countStar <= MinStar)
                {
                    _isFeedback = true;
                    return;
                }

                var ok = EditorUtility.DisplayDialog("Please take a look", "Do you have a github account?",
                    "Rate me on Github", "Rate me on AssetStore");
                if (ok)
                    Application.OpenURL(githubUrl);
                else
                    Application.OpenURL(_assetStoreUrl);
            }

            if (Event.current.type == EventType.Repaint)
            {
                var countStar = _countStar;

                for (var i = 0; i < MaxStar; i++)
                    if (_starRects[i].Contains(Event.current.mousePosition))
                        countStar = i + 1;

                SetCountStar(countStar);
            }

            //Repaint();
        }
       
        
        private void SetCountStar(int countStar)
        {
            for (var i = 0; i <  MaxStar; i++)
                _targetGui.Assets.StarStyles[i] = i < countStar ? 
                    _targetGui.Assets.StarredButtonStyle : 
                    _targetGui.Assets.UnstarredButtonStyle;
        }
        
        
        private readonly string additional = "\n\n\n\n________\n\nPlease Do Not Modify This\n\n{0}\n\n________\n";
        private readonly string email = "s.khalandachev@gmail.com";
        private readonly string githubUrl = "";
        private readonly string subject = "[Support] SKTools.MenuItemsFinder FEEDBACK/SUGGESTION";

        public void EmailMe()
        {
            var builder = new StringBuilder();
            builder.Append("mailto:" + email);
            builder.Append("?subject=" + EscapeURL(subject));
            builder.Append("&body=" + EscapeURL(_message + additional));
            Application.OpenURL(builder.ToString());
        }

        private string EscapeURL(string url)
        {
            return WWW.EscapeURL(url).Replace("+", "%20");
        }
    }
}