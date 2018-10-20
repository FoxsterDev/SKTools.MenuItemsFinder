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
        private const int MaxStar = 5;
        private const int SizeStar = 64;
          
        private GUIStyle _labelStyle;
        private GUIStyle _unstarredButtonStyle, _starredButtonStyle;

        private int _countStar = 0;

        private bool isFeedback = false;
        private string Message = "Please Enter your message here";
        private Rect[] _rects = new Rect[MaxStar];
        private GUIStyle[] _starStyles = new GUIStyle[MaxStar];

        [MenuItem("SKTools/Rate Me")]
        private static void ShowRateMeWindow()
        {
            var window = GetWindow<RateMeWindow>(false, "RateMe", true);
            window.minSize = new Vector2(400, 400);
            window.maxSize = new Vector2(400, 400);
            
            //window.position = new Rect((Screen.width-00)/2, (Screen.height- 400)/2, 400, 400);
            var starredImage = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/SKTools/MenuItemsFinder/Editor Resources/starred.png");
            var unstarredImage = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/SKTools/MenuItemsFinder/Editor Resources/unstarred.png");

            var unstarredButtonStyle = new GUIStyle();
            unstarredButtonStyle.fixedHeight = SizeStar;
            unstarredButtonStyle.fixedWidth = SizeStar;
            unstarredButtonStyle.stretchHeight = false;
            unstarredButtonStyle.stretchWidth = false;

            unstarredButtonStyle.active.background =
                unstarredButtonStyle.focused.background =
                    unstarredButtonStyle.hover.background =
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
            
            if (isFeedback)
            {
                GUI.Label(new Rect(5, 5, position.width, 24),"FEEDBACK/SUGGESTION");
                Message = GUI.TextArea(new Rect(5, 24, position.width - 10, position.height - 128), Message);
                
                if (GUI.Button(new Rect(position.width/2 - 156, position.height - 64, 156, 64), "Back",
                    buttonStyle))
                {
                    isFeedback = false;
                }

                if (GUI.Button(new Rect(position.width/2 + 5, position.height - 64, 156, 64), "Send Email",
                    buttonStyle))
                {
                    EmailUs();
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
            
            
            
            GUI.Label(rectUpper, "How do you like it?", labelStyle);//, GUILayout.ExpandWidth(true));

            var spaceX = (position.width / MaxStar - SizeStar) / 2;
            var rect = new Rect(0, height, SizeStar, SizeStar);
            
            for (var i = 0; i < MaxStar; i++)
            {
                rect.x += spaceX;
                _rects[i] = rect;
                if (GUI.Button(_rects[i], string.Empty, _starStyles[i]))
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
                    isFeedback = true;
                    return;
                }
                else
                {
                    var ok = (EditorUtility.DisplayDialog("Please take a look", "Do you have a github account?",
                        "Rate me on Github", "Rate me on AssetStore")) ;
                    //Application.OpenURL("");
                }
            }
            
            if (Event.current.type == EventType.Repaint)
            {
                var countStar = _countStar;
                
                for (var i = 0; i < MaxStar; i++)
                {
                    if (_rects[i].Contains(Event.current.mousePosition))
                    {
                        countStar = i + 1;
                    }
                }
                    
                SetCountStar(countStar);
            }

            Repaint();
        }
        
        
        private void SetCountStar(int star)
        {
            for (var i = 0; i < MaxStar; i++)
                _starStyles[i] = i < star ? _starredButtonStyle : _unstarredButtonStyle;
        }

    
        public void EmailUs () 
        {
            //unity version
            //additional info
            var builder = new StringBuilder();
            builder.Append("mailto:" + "s.khalandachev@gmail.com");
            builder.Append("?subject=" + MyEscapeURL("[Support] SKTools.MenuItemsFinder FEEDBACK/SUGGESTION"));
            builder.Append("&body=" + MyEscapeURL(Message +"\n\n\n\n" +
                                                  "________" +
                                                  "\n\nPlease Do Not Modify This\n\n" +
                                                  "Model: "+SystemInfo.deviceModel+"\n\n"+
                                                  "OS: "+SystemInfo.operatingSystem+"\n\n" +
                                                  "________"));
            //Debug.Log(builder.ToString());
            Application.OpenURL(builder.ToString());
        }  
  
        private string MyEscapeURL (string url)
        {
            return WWW.EscapeURL(url).Replace("+","%20");
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