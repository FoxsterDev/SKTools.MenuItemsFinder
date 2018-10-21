using System;
using System.Diagnostics;
using System.IO;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace SKTools.RateMeWindow
{
    //good example https://github.com/Tayx94/graphy
    internal class RateMeWindow : EditorWindow
    {
        private string name = "it";
        private string githubUrl = "";
        private string assetStoreUrl;
        private string email = "s.khalandachev@gmail.com";
        private string subject = "[Support] SKTools.MenuItemsFinder FEEDBACK/SUGGESTION";
        private string additional = "\n\n\n\n________\n\nPlease Do Not Modify This\n\n{0}\n\n________\n";

        
        private const int MaxStar = 5;
        private const int SizeStar = 64;
          
        private GUIStyle _labelStyle;
        private GUIStyle _unstarredButtonStyle, _starredButtonStyle;
        private GUIStyle[] _starStyles = new GUIStyle[MaxStar];

        private int _countStar = 0;

        private bool _isFeedback = false;
        private string _message = "Please Enter your message here";
        private Rect[] _starRects = new Rect[MaxStar];
        
        [MenuItem("SKTools/Rate Me")]
        private static void ShowRateMeWindow()
        {
            var window = GetWindow<RateMeWindow>(false, "RateMe", true);
            window.minSize = new Vector2(400, 400);
            window.maxSize = new Vector2(400, 400);
            
            var starredImage = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/SKTools/MenuItemsFinder/Editor Resources/starred.png");
            var unstarredImage = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/SKTools/MenuItemsFinder/Editor Resources/unstarred.png");

            var unstarredButtonStyle = new GUIStyle();
            unstarredButtonStyle.fixedHeight = SizeStar;
            unstarredButtonStyle.fixedWidth = SizeStar;
            unstarredButtonStyle.stretchHeight = false;
            unstarredButtonStyle.stretchWidth = false;

            unstarredButtonStyle.focused.background =
                unstarredButtonStyle.normal.background = unstarredImage;

            unstarredButtonStyle.active.background =
                unstarredButtonStyle.hover.background = starredImage;

            var starredButtonStyle = new GUIStyle(unstarredButtonStyle);

            starredButtonStyle.active.background =
                starredButtonStyle.focused.background =
                    starredButtonStyle.hover.background =
                        starredButtonStyle.normal.background = starredImage;

            window._unstarredButtonStyle = unstarredButtonStyle;
            window._starredButtonStyle = starredButtonStyle;
            
            window._starStyles = new GUIStyle[MaxStar];

            window.SetCountStar(1);
        }

        private void OnGUI()
        {
            var buttonStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = 24
            };
            
            if (_isFeedback)
            {
                GUI.Label(new Rect(5, 5, position.width, 24),"FEEDBACK/SUGGESTION");
                _message = GUI.TextArea(new Rect(5, 24, position.width - 10, position.height - 128), _message);
                
                if (GUI.Button(new Rect(position.width/2 - 156, position.height - 64, 156, 64), "Back",
                    buttonStyle))
                {
                    _isFeedback = false;
                }

                if (GUI.Button(new Rect(position.width/2 + 5, position.height - 64, 156, 64), "Send Email",
                    buttonStyle))
                {
                    EmailMe();
                }
                return;
            }
            
            var height = (position.height - SizeStar) / 2;
            var rectUpper = new Rect(0, 0, position.width, height);

            var labelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = 35
            };
            
            GUI.Label(rectUpper, "How do you like it?", labelStyle);

            var spaceX = (position.width / MaxStar - SizeStar) / 2;
            var rect = new Rect(0, height, SizeStar, SizeStar);
            
            for (var i = 0; i < MaxStar; i++)
            {
                rect.x += spaceX;
                _starRects[i] = rect;
                if (GUI.Button(_starRects[i], string.Empty, _starStyles[i]))
                {
                    _countStar = i + 1;
                    SetCountStar(_countStar);
                    return;
                }

                rect.x += SizeStar + spaceX;
            }
            
            if (GUI.Button(new Rect((position.width - 128)/2, position.height - 64, 128, 64), "Rate", buttonStyle))
            {
                if (_countStar < 4)
                {
                    _isFeedback = true;
                    return;
                }
                else
                {
                    var ok = (EditorUtility.DisplayDialog("Please take a look", "Do you have a github account?",
                        "Rate me on Github", "Rate me on AssetStore")) ;
                    if (ok)
                    {
                        Application.OpenURL(githubUrl);
                    }
                    else
                    {
                        Application.OpenURL(assetStoreUrl);
                    }
                }
            }
            
            if (Event.current.type == EventType.Repaint)
            {
                var countStar = _countStar;
                
                for (var i = 0; i < MaxStar; i++)
                {
                    if (_starRects[i].Contains(Event.current.mousePosition))
                    {
                        countStar = i + 1;
                    }
                }
                    
                SetCountStar(countStar);
            }

            Repaint();
        }
        
        private void SetCountStar(int countStar)
        {
            for (var i = 0; i < MaxStar; i++)
                _starStyles[i] = i < countStar ? _starredButtonStyle : _unstarredButtonStyle;
        }
        
        public void EmailMe () 
        {
            var builder = new StringBuilder();
            builder.Append("mailto:" + email);
            builder.Append("?subject=" + MyEscapeURL(subject));
            builder.Append("&body=" + MyEscapeURL(_message + additional));
            Application.OpenURL(builder.ToString());
        }  
  
        private string MyEscapeURL (string url)
        {
            return WWW.EscapeURL(url).Replace("+","%20");
        }
    }
}