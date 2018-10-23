using SKTools.Base.Editor;
using UnityEngine;

namespace SKTools.RateMeWindow
{
    internal class Assets : AssetsContainer
    {
        public const int MaxStar = 5;
        public const int SizeStar = 64;
        
        private GUIStyle _labelStyle, _buttonStyle;
        private GUIStyle _unstarredButtonStyle, _starredButtonStyle;
        private GUIStyle[] _starStyles;
        
        public Assets(string assetsDirectory) : base(assetsDirectory)
        {
        }

        private Texture2D UnstarredImage
        {
            get { return Get<Texture2D>("unstarred"); }
        }

        private Texture2D StarredImage
        {
            get { return Get<Texture2D>("starred"); }
        }

        public GUIStyle LabelStyle
        {
            get
            {
                if (_labelStyle == null)
                {
                    _labelStyle = new GUIStyle(GUI.skin.label)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fontStyle = FontStyle.Bold,
                        fontSize = 35
                    };
                }

                return _labelStyle;
            }
        }

        public GUIStyle ButtonStyle
        {
            get
            {
                if (_buttonStyle == null)
                    _buttonStyle = new GUIStyle(GUI.skin.button)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fontStyle = FontStyle.Bold,
                        fontSize = 24
                    };

                return _buttonStyle;
            }
        }

        public GUIStyle UnstarredButtonStyle
        {
            get
            {
                if (_unstarredButtonStyle == null)
                {
                    _unstarredButtonStyle = new GUIStyle();
                    _unstarredButtonStyle.fixedHeight = SizeStar;
                    _unstarredButtonStyle.fixedWidth = SizeStar;
                    _unstarredButtonStyle.stretchHeight = false;
                    _unstarredButtonStyle.stretchWidth = false;

                    _unstarredButtonStyle.focused.background =
                        _unstarredButtonStyle.normal.background = UnstarredImage;

                    _unstarredButtonStyle.active.background =
                        _unstarredButtonStyle.hover.background = StarredImage;
                }

                return _unstarredButtonStyle;
            }
        }

        public GUIStyle StarredButtonStyle
        {
            get
            {
                if (_starredButtonStyle == null)
                {
                    _starredButtonStyle = new GUIStyle();
                    _starredButtonStyle.fixedHeight = SizeStar;
                    _starredButtonStyle.fixedWidth = SizeStar;
                    _starredButtonStyle.stretchHeight = false;
                    _starredButtonStyle.stretchWidth = false;

                    _starredButtonStyle.active.background =
                        _starredButtonStyle.focused.background =
                            _starredButtonStyle.hover.background =
                                _starredButtonStyle.normal.background = StarredImage;
                }

                return _starredButtonStyle;
            }
        }

        public GUIStyle[] StarStyles
        {
            get { return _starStyles ?? (_starStyles = new GUIStyle[MaxStar]); }
        }
    }
}