using System;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace SKTools.MenuItemsFinder
{
    internal class MenuItemsFinderWindow : EditorWindow
    {
        private Vector2 _scrollPosition;

        private GUIStyle _menuItemButtonStyle,
            _unstarredMenuItemButtonStyle,
            _starredMenuItemButtonStyle,
            _settingsMenuItemButtonStyle;

        private MenuItemsFinder _finder;

        [MenuItem("SKTools/MenuItems Finder %#m")]
        private static void Init()
        {
            var finderWindow = (MenuItemsFinderWindow) GetWindow(typeof(MenuItemsFinderWindow), false,
                typeof(MenuItemsFinderWindow).Name);
            finderWindow.autoRepaintOnSceneChange = true;
            finderWindow.Show();
        }

        private void Awake()
        {
            _finder = new MenuItemsFinder();
            _finder.Load();
            CreateStyles();
        }

        private void OnGUI()
        {
            if (IsCompiling())
            {
                return;
            }

            CheckMissedFinder();

            DrawSearchTextField();
            DrawMenuOptions();
            DrawItems();
        }

        private void OnSelectionChange()
        {
            Repaint();
        }

        private void OnDestroy()
        {
            _finder.SavePrefs();
        }

        private void OnLostFocus()
        {
            _finder.SavePrefs();
        }

        /// <summary>
        /// Need after recompiling
        /// </summary>
        private void CheckMissedFinder()
        {
            if (_finder == null) //after recompiling and remaining opened window
            {
                _finder = new MenuItemsFinder();
                _finder.Load();
            }
        }

        private bool IsCompiling()
        {
            if (EditorApplication.isCompiling)
            {
                //pivot = new Vector2(position.xMin + position.width * 0.5f, position.yMin + position.height * 0.5f);
                //var matrixBackup = GUI.matrix;
                //GUIUtility.RotateAroundPivot(angle%360, pivot);
                var width = _finder.LoadingImage.width;
                var height = _finder.LoadingImage.height;
                var rect = new Rect(position.width * 0.5f - width * 0.5f, position.height * 0.5f - height * 0.5f, width,
                    height);
                GUI.DrawTexture(rect, _finder.LoadingImage);
                //GUI.matrix = matrixBackup;
                return true;
            }

            return false;
        }

        private void DrawSearchTextField()
        {
            _finder.Prefs.FilterString =
                GUILayoutCollection.SearchTextField(_finder.Prefs.FilterString, GUILayout.MinWidth(200));

            if (!_finder.Prefs.FilterString.Equals(_finder.Prefs.PreviousFilterString))
            {
                var key = _finder.Prefs.FilterString.ToLower();
                _finder.Prefs.PreviousFilterString = _finder.Prefs.FilterString;
                _finder.FilteredMenuItems = _finder.MenuItems.FindAll(m => m.Key.Contains(key));
            }
        }

        private void DrawMenuOptions()
        {
            GUILayout.BeginHorizontal();

            _finder.Prefs.OnlyWithValidate = GUILayout.Toggle(_finder.Prefs.OnlyWithValidate, "=OnlyWithValidate",
                GUILayout.MinWidth(100));

            if (GUILayout.Button("All Unstarred", GUILayout.MaxWidth(100)))
            {
                _finder.AllUnstarred();
            }

            GUILayout.EndHorizontal();
        }

        private void DrawItems()
        {
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, true);

            _finder.MenuItems.FindAll(m => m.Starred).ForEach(Draw);
            _finder.FilteredMenuItems.FindAll(m => !m.Starred).ForEach(Draw);

            GUILayout.EndScrollView();
        }

        private MenuItemLink _current;

        private void Draw(MenuItemLink item)
        {
            if (_finder.Prefs.OnlyWithValidate && !item.HasValidate)
            {
                return;
            }

            var defaultColor = item.Starred ? Color.green : Color.white;
            bool? validated;
            if (item.HasValidate)
            {
                try
                {
                    validated = item.CanExecute();
                }
                catch
                {
                    validated = false;
                }

                if (validated == false) defaultColor = Color.gray;
            }

            GUILayout.BeginHorizontal();

            var previousColor = GUI.color;
            GUI.color = defaultColor;

            if (GUILayout.Button(item.Label, _menuItemButtonStyle, GUILayout.MaxWidth(300)))
            {
                Debug.Log("Try execute menuItem=" + item);
                try
                {
                    if (validated == false)
                    {
                        EditorUtility.DisplayDialog("Validation fail!", "Cant validate this option\n" + item.Label,
                            "ok");
                        return;
                    }

                    item.Execute();
                }
                catch (Exception ex)
                {
                    Debug.LogError("cant execute this menu item: " + item + "\n" + ex);
                }
            }

            GUI.color = previousColor;

            if (GUILayout.Button(string.Empty,
                item.Starred ? _starredMenuItemButtonStyle : _unstarredMenuItemButtonStyle))
            {
                _finder.ToggleStarred(item);
            }

            if (GUILayout.Button(string.Empty, _settingsMenuItemButtonStyle))
            {
                if (_current == null || _current.Key != item.Key)
                {
                    _current = item;
                }
                else
                {
                    _current = null;
                }
            }

            GUILayout.EndHorizontal();

            if (_current != null && _current.Key.Equals(item.Key))
            {
                GUILayout.BeginHorizontal();

                GUILayout.Label("Custom name", GUILayout.MinWidth(80), GUILayout.MaxWidth(80));
                _current.CustomName =
                    GUILayout.TextField(_current.CustomName, GUILayout.MinWidth(150), GUILayout.MaxWidth(150));
                GUILayout.Label("");

                GUILayout.EndHorizontal();
            }
        }

        private void DrawMenuItemCurrent(MenuItemLink item)
        {
        }

        private void CreateStyles()
        {
            _menuItemButtonStyle = new GUIStyle(EditorStyles.miniButton);
            _menuItemButtonStyle.fixedHeight = 20;
            //_menuItemButtonStyle.fixedWidth = 200;
            _menuItemButtonStyle.alignment = TextAnchor.MiddleLeft;
            _menuItemButtonStyle.richText = true;

            _unstarredMenuItemButtonStyle = new GUIStyle(EditorStyles.miniButton);
            _unstarredMenuItemButtonStyle.fixedHeight = 32;
            _unstarredMenuItemButtonStyle.fixedWidth = 32;
            _unstarredMenuItemButtonStyle.stretchHeight = false;
            _unstarredMenuItemButtonStyle.stretchWidth = false;
            _unstarredMenuItemButtonStyle.imagePosition = ImagePosition.ImageOnly;
            _unstarredMenuItemButtonStyle.overflow = new RectOffset(0, 0, 8, -6);
            _unstarredMenuItemButtonStyle.active.background =
                _unstarredMenuItemButtonStyle.focused.background =
                    _unstarredMenuItemButtonStyle.hover.background =
                        _unstarredMenuItemButtonStyle.normal.background = _finder.UnstarredImage;

            _starredMenuItemButtonStyle = new GUIStyle(_unstarredMenuItemButtonStyle);

            _starredMenuItemButtonStyle.active.background =
                _starredMenuItemButtonStyle.focused.background =
                    _starredMenuItemButtonStyle.hover.background =
                        _starredMenuItemButtonStyle.normal.background = _finder.StarredImage;

            _settingsMenuItemButtonStyle = new GUIStyle(_unstarredMenuItemButtonStyle);
            _settingsMenuItemButtonStyle.active.background =
                _settingsMenuItemButtonStyle.focused.background =
                    _settingsMenuItemButtonStyle.hover.background =
                        _settingsMenuItemButtonStyle.normal.background = _finder.SettingsImage;
        }
    }
}