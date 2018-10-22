using SKTools.Base.Editor;
using UnityEditor;
using UnityEngine;

namespace SKTools.MenuItemsFinder
{
    internal class MenuItemsFinderAssetsContainer : AssetsContainer
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
                        fixedHeight = 20, alignment = TextAnchor.MiddleLeft, richText = true
                    };
                }

                return _menuItemButtonStyle;
            }
        }

        public GUIStyle UnstarredMenuItemButtonStyle
        {
            get
            {
                if (_unstarredMenuItemButtonStyle == null)
                {
                    _unstarredMenuItemButtonStyle = new GUIStyle
                    {
                        fixedHeight = 32, fixedWidth = 32, overflow = new RectOffset(0, 0, 8, -6)
                    };

                    _unstarredMenuItemButtonStyle.active.background =
                        _unstarredMenuItemButtonStyle.focused.background =
                            _unstarredMenuItemButtonStyle.hover.background =
                                _unstarredMenuItemButtonStyle.normal.background = UnstarredImage;
                }

                return _unstarredMenuItemButtonStyle;
            }
        }

        public GUIStyle StarredMenuItemButtonStyle
        {
            get
            {
                if (_starredMenuItemButtonStyle == null)
                {
                    _starredMenuItemButtonStyle = new GUIStyle
                    {
                        fixedHeight = 32, fixedWidth = 32, overflow = new RectOffset(0, 0, 8, -6)
                    };
                    _starredMenuItemButtonStyle.active.background =
                        _starredMenuItemButtonStyle.focused.background =
                            _starredMenuItemButtonStyle.hover.background =
                                _starredMenuItemButtonStyle.normal.background = StarredImage;
                }

                return _starredMenuItemButtonStyle;
            }
        }

        public GUIStyle SettingsMenuItemButtonStyle
        {
            get
            {
                if (_settingsMenuItemButtonStyle == null)
                {
                    _settingsMenuItemButtonStyle = new GUIStyle();
                    _starredMenuItemButtonStyle.fixedHeight = 32;
                    _starredMenuItemButtonStyle.fixedWidth = 32;
                    _settingsMenuItemButtonStyle.active.background =
                        _settingsMenuItemButtonStyle.focused.background =
                            _settingsMenuItemButtonStyle.hover.background =
                                _settingsMenuItemButtonStyle.normal.background = SettingsImage;
                }

                return _settingsMenuItemButtonStyle;
            }
        }
    }
}