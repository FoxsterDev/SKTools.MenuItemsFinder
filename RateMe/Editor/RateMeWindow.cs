using UnityEditor;
using UnityEngine;

namespace SKTools.RateMeWindow
{
    internal class RateMeWindow : EditorWindow
    {
        private const int maxStar = 5;

        private GUIStyle _labelStyle;
        private GUIStyle
            _unstarredMenuItemButtonStyle,
            _starredMenuItemButtonStyle;

        private int countStar = 0;

        private readonly Rect[] _rects = new Rect[maxStar];
        public Texture2D StarredImage, UnstarredImage;
        private GUIStyle[] _starStyles = new GUIStyle[maxStar];

        [MenuItem("SKTools/Rate Me")]
        private static void ShowWindow()
        {
            
            var window = GetWindow<RateMeWindow>(false, "RateMe", true);
            window.minSize = new Vector2(400, 400);

            //window.UnstarredImage = LoadAsset<Texture2D>(assetsPath, "unstarred.png");
            window.StarredImage =
                AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/SKTools/MenuItemsFinder/Editor Resources/starred.png");
            window.UnstarredImage =
                AssetDatabase.LoadAssetAtPath<Texture2D>(
                    "Assets/SKTools/MenuItemsFinder/Editor Resources/unstarred.png");

            var _unstarredMenuItemButtonStyle = new GUIStyle();
            _unstarredMenuItemButtonStyle.fixedHeight = 64;
            _unstarredMenuItemButtonStyle.fixedWidth = 64;
            _unstarredMenuItemButtonStyle.stretchHeight = false;
            _unstarredMenuItemButtonStyle.stretchWidth = false;

            _unstarredMenuItemButtonStyle.active.background =
                _unstarredMenuItemButtonStyle.focused.background =
                    _unstarredMenuItemButtonStyle.hover.background =
                        _unstarredMenuItemButtonStyle.normal.background = window.UnstarredImage;

            _unstarredMenuItemButtonStyle.active.background =
                _unstarredMenuItemButtonStyle.hover.background = window.StarredImage;

            var _starredMenuItemButtonStyle = new GUIStyle(_unstarredMenuItemButtonStyle);

            _starredMenuItemButtonStyle.active.background =
                _starredMenuItemButtonStyle.focused.background =
                    _starredMenuItemButtonStyle.hover.background =
                        _starredMenuItemButtonStyle.normal.background = window.StarredImage;

            window._unstarredMenuItemButtonStyle = _unstarredMenuItemButtonStyle;
            window._starredMenuItemButtonStyle = _starredMenuItemButtonStyle;
            window._starStyles = new GUIStyle[maxStar];

            window.SetCountStar(1);
        }

        private void OnGUI()
        {
            var labelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = 35
            };
            
            var buttonStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = 24
            };
            
            var height = (position.height - 64) / 2;
            GUI.Label(new Rect(0, 0, position.width, height), "How do you like it?", labelStyle);//, GUILayout.ExpandWidth(true));

            var spaceX = (position.width / maxStar - 64) / 2;
            var rect = new Rect(0, height, 64, 64);
            
            for (var i = 0; i < maxStar; i++)
            {
                rect.x += spaceX;
                _rects[i] = rect;
                if (GUI.Button(_rects[i], string.Empty, _starStyles[i]))
                {
                    countStar = i + 1;
                    SetCountStar(countStar);
                    return;
                }

                rect.x += 64 + spaceX;
            }
            
            if (GUI.Button(new Rect((position.width - 128)/2, position.height - 64, 128, 64), "Rate", buttonStyle))
            {
                if (countStar < 4)
                {
                    /*
                     * textarea
                     * back button
                     * email button and close it
                     */
                }
                else
                {
                    var pk = (EditorUtility.DisplayDialog("Please", "Do you have a github account?",
                        "Rate me on Github", "Rate me on AssetStore")) ;
                    //Application.OpenURL("");
                }
            }
            

            if (Event.current.type == EventType.Repaint)
            {
                for (var i = 0; i < maxStar; i++)
                {
                    if (_rects[i].Contains(Event.current.mousePosition))
                    {
                        SetCountStar(i+1);
                        return;
                    }
                }
                    
                SetCountStar(countStar);
            }

            Repaint();
        }
        
        
        private void SetCountStar(int star)
        {
            for (var i = 0; i < maxStar; i++)
                _starStyles[i] = i < star ? _starredMenuItemButtonStyle : _unstarredMenuItemButtonStyle;
        }

    
        private void OnSelectionChange()
        {
            //Repaint();
        }

        private void Update()
        {
            //Repaint();
        }
    }
}