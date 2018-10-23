using System.Text;
using SKTools.Base.Editor;
using UnityEditor;
using UnityEngine;

namespace SKTools.RateMeWindow
{
//good example https://github.com/Tayx94/graphy
    internal class RateMeWindowObsolete : EditorWindow
    {
        private const int MaxStar = 5;
        private const int SizeStar = 64;
        private const int MinStar = 3;

        private static Assets _assets;
        private string _assetStoreUrl;

        private int _countStar;

        private bool _isFeedback;
        private string _message = "Please Enter your message here";

        private string _name = "it";

        private readonly Rect[] _starRects = new Rect[MaxStar];
       
        private readonly string additional = "\n\n\n\n________\n\nPlease Do Not Modify This\n\n{0}\n\n________\n";
        private readonly string email = "s.khalandachev@gmail.com";
        private readonly string githubUrl = "";
        private readonly string subject = "[Support] SKTools.MenuItemsFinder FEEDBACK/SUGGESTION";

        [MenuItem("SKTools/Rate Me")]
        private static void ShowRateMeWindow()
        {
            var assetsDirectory = Utility.GetPath("Editor Resources");
            _assets = new Assets(assetsDirectory);
            _assets.Load();

            var window = GetWindow<RateMeWindowObsolete>(false, "RateMe", true);
            window.minSize = new Vector2(400, 400);
            window.maxSize = new Vector2(400, 400);

            window.SetCountStar(1);
        }

        private void OnGUI()
        {
            if (_isFeedback)
            {
                GUI.Label(new Rect(5, 5, position.width, 24), "FEEDBACK/SUGGESTION");
                _message = GUI.TextArea(new Rect(5, 24, position.width - 10, position.height - 128), _message);

                if (GUI.Button(new Rect(position.width / 2 - 156, position.height - 64, 156, 64), "Back",
                    _assets.ButtonStyle))
                    _isFeedback = false;

                if (GUI.Button(new Rect(position.width / 2 + 5, position.height - 64, 156, 64), "Send Email",
                    _assets.ButtonStyle))
                    EmailMe();

                return;
            }

            var height = (position.height - SizeStar) / 2;
            var rectUpper = new Rect(0, 0, position.width, height);

            GUI.Label(rectUpper, "How do you like it?", _assets.LabelStyle);

            var spaceX = (position.width / MaxStar - SizeStar) / 2;
            var rect = new Rect(0, height, SizeStar, SizeStar);

            for (var i = 0; i < MaxStar; i++)
            {
                rect.x += spaceX;
                _starRects[i] = rect;
                if (GUI.Button(_starRects[i], string.Empty, _assets.StarStyles[i]))
                {
                    _countStar = i + 1;
                    SetCountStar(_countStar);
                    return;
                }

                rect.x += SizeStar + spaceX;
            }

            if (GUI.Button(new Rect((position.width - 128) / 2, position.height - 64, 128, 64), "Rate",
                _assets.ButtonStyle))
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

            Repaint();
        }

        private void SetCountStar(int countStar)
        {
            for (var i = 0; i < MaxStar; i++)
                _assets.StarStyles[i] = i < countStar ? _assets.StarredButtonStyle : _assets.UnstarredButtonStyle;
        }

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