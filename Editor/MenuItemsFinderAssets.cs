using SKTools.Base.Editor;
using UnityEditor;
using UnityEngine;

namespace SKTools.MenuItemsFinder
{
    internal class Assets : AssetsContainer
    {
        private GUIStyle _menuItemButtonStyle;
        private GUIStyle _settingsMenuItemButtonStyle;
        private GUIStyle _starredMenuItemButtonStyle;
        private GUIStyle _unstarredMenuItemButtonStyle;

        public Texture2D UnstarredImage
        {
            get { return Get<Texture2D>("unstarred"); }
        }

        public Texture2D StarredImage
        {
            get { return Get<Texture2D>("starred"); }
        }

        public Texture2D LoadingImage
        {
            get { return Get<Texture2D>("loading"); }
        }

        public Texture2D SettingsImage
        {
            get { return Get<Texture2D>("settings"); }
        }
        
        public GUIStyle MenuItemButtonStyle
        {
            get
            {
                if (_menuItemButtonStyle == null)
                {
                    _menuItemButtonStyle = new GUIStyle(EditorStyles.miniButton)
                    {
                        fixedHeight = 20, alignment = TextAnchor.MiddleLeft, richText = true,
                        fixedWidth = 400
                    };
                }

                return _menuItemButtonStyle;
            }
        }

        public Assets(string assetsDirectory) : base(assetsDirectory)
        {
        }
    }
}