using System;
using System.Collections.Generic;
using System.IO;
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
            //DrawMenuOptions();
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
            _finder.Prefs.FilterString = GUILayoutCollection.SearchTextField(_finder.Prefs.FilterString,
                _finder.RolledOutMenuItem == null, GUILayout.MinWidth(200));

            if (!_finder.Prefs.FilterString.Equals(_finder.Prefs.PreviousFilterString))
            {
                 var key = _finder.Prefs.FilterString.ToLower();
                _finder.Prefs.PreviousFilterString = _finder.Prefs.FilterString;
                _finder.FilteredMenuItems.Clear();

                foreach (var item in _finder.MenuItems)
                {
                    if (item.Key.Contains(key))
                    {
                        _finder.FilteredMenuItems.Add(item);
                        continue;
                    }
                    
                    if (string.IsNullOrEmpty(item.CustomName))
                        continue;
                    
                    if (item.CustomName.Contains(key))
                    {
                        _finder.FilteredMenuItems.Add(item);
                        continue;
                    }
                }
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

            _finder.MenuItems.FindAll(m => m.Starred).ForEach(DrawItem);
            _finder.FilteredMenuItems.FindAll(m => !m.Starred).ForEach(DrawItem);

            GUILayout.EndScrollView();
        }

        private void DrawItem(MenuItemLink item)
        {
            if (_finder.Prefs.OnlyWithValidate && !item.HasValidate)
            {
                return;
            }

            var defaultColor = item.Starred ? Color.green : Color.white;
            bool? validated = null;
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
            //GUI.contentColor = defaultColor;
            //_menuItemButtonStyle.normal.c
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

            GUI.contentColor =GUI.color = previousColor;

            var texture = (item.Starred ? _finder.StarredImage : _finder.UnstarredImage);
            if (GUILayout.Button(texture, GUILayout.MaxWidth(24), GUILayout.MaxHeight(24)))
            {
                _finder.ToggleStarred(item);
                return;
            }

            if (GUILayout.Button(_finder.SettingsImage, GUILayout.MaxWidth(24), GUILayout.MaxHeight(24)))
            {
                if (_finder.RolledOutMenuItem == null || _finder.RolledOutMenuItem.Key != item.Key)
                {
                    GUI.FocusControl("RolledOutMenuItemCustomName");
                    _finder.RolledOutMenuItem = item;
                }
                else
                {
                    _finder.RolledOutMenuItem = null;
                }
            }

            GUILayout.EndHorizontal();

            if (_finder.RolledOutMenuItem != null)
            {
                if (_finder.RolledOutMenuItem.Key.Equals(item.Key))
                {
                    DrawSettings(item);
                }
            }
        }

        private void DrawSettings(MenuItemLink item)
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Try open file", GUILayout.MinWidth(80), GUILayout.MaxWidth(80)))
            {
                var error = "";
                _finder.TryOpenFileThatContainsMenuItem(item, out error);
                if (!string.IsNullOrEmpty(error))
                {
                    var ok = EditorUtility.DisplayDialog("Can't open file that contains this menuItem", 
                        "There happens tgis="+error+"\n Do ypu want to open location of assembly?", "ok", "cancel");
                    if (ok)
                    {
                        _finder.OpenAssemblyLocationThatContainsMenuItem(item);
                    }
                }
            }
            
            GUILayout.Label("Custom name:", GUILayout.MinWidth(80), GUILayout.MaxWidth(80));

            GUI.SetNextControlName("RolledOutMenuItemCustomName");
                    
            _finder.RolledOutMenuItem.CustomNameEditable = GUILayout.TextField(
                _finder.RolledOutMenuItem.CustomNameEditable,
                GUILayout.MinWidth(150), GUILayout.MaxWidth(150));

            if (GUILayout.Button("+", GUILayout.MinWidth(20), GUILayout.MaxWidth(20)))
            {
                _finder.AddCustomizedNameToPrefs(item);
            }

            GUILayout.EndHorizontal();
        }
        
        private void CreateStyles()
        {
            _menuItemButtonStyle = new GUIStyle(EditorStyles.miniButton);
            _menuItemButtonStyle.fixedHeight = 20;
            //_menuItemButtonStyle.fixedWidth = 200;
            _menuItemButtonStyle.alignment = TextAnchor.MiddleLeft;
            _menuItemButtonStyle.richText = true;
           
            _unstarredMenuItemButtonStyle = new GUIStyle();
            _unstarredMenuItemButtonStyle.fixedHeight = 32;
            _unstarredMenuItemButtonStyle.fixedWidth = 32;
            _unstarredMenuItemButtonStyle.stretchHeight = true;
            _unstarredMenuItemButtonStyle.stretchWidth = true;
            //_unstarredMenuItemButtonStyle.imagePosition = ImagePosition.ImageOnly;
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
            _settingsMenuItemButtonStyle.overflow = new RectOffset();
            _settingsMenuItemButtonStyle.active.background =
                _settingsMenuItemButtonStyle.focused.background =
                    _settingsMenuItemButtonStyle.hover.background =
                        _settingsMenuItemButtonStyle.normal.background = _finder.SettingsImage;
        }
    }
}