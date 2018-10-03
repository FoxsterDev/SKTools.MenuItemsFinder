using System;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace SKTools.MenuItemsFinder
{
    internal class MenuItemsFinderWindow : EditorWindow
    {
        private Vector2 _scrollPosition;
        private GUIStyle _menuItemButtonStyle, _unstarredMenuItemButtonStyle, _starredMenuItemButtonStyle;
        private MenuItemsFinder _finder;

        [MenuItem("SKTools/MenuItems Finder %#m")]
        private static void Init()
        {
            var finderWindow = (MenuItemsFinderWindow) GetWindow(typeof(MenuItemsFinderWindow), false,
                typeof(MenuItemsFinderWindow).Name);
            finderWindow.autoRepaintOnSceneChange = true;
            finderWindow.Show();
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
            _unstarredMenuItemButtonStyle.overflow = new RectOffset(0, 0, 6, -6);
            _unstarredMenuItemButtonStyle.active.background =
                _unstarredMenuItemButtonStyle.focused.background =
                    _unstarredMenuItemButtonStyle.hover.background =
                        _unstarredMenuItemButtonStyle.normal.background = _finder.UnstarredImage;


            _starredMenuItemButtonStyle = new GUIStyle(_unstarredMenuItemButtonStyle);

            _starredMenuItemButtonStyle.active.background =
                _starredMenuItemButtonStyle.focused.background =
                    _starredMenuItemButtonStyle.hover.background =
                        _starredMenuItemButtonStyle.normal.background = _finder.StarredImage;
        }

        private void Awake()
        {
            _finder = new MenuItemsFinder();
            _finder.Load();
             CreateStyles();
        }
        
        private void OnGUI()
        {
            if (EditorApplication.isCompiling)
            {
                //pivot = new Vector2(position.xMin + position.width * 0.5f, position.yMin + position.height * 0.5f);
                //var matrixBackup = GUI.matrix;
                //GUIUtility.RotateAroundPivot(angle%360, pivot);
                var width = _finder.LoadingImage.width;
                var height = _finder.LoadingImage.height;
                var rect = new Rect(position.width * 0.5f - width*0.5f, position.height * 0.5f - height*0.5f, width, height);
                GUI.DrawTexture(rect, _finder.LoadingImage);   
                //GUI.matrix = matrixBackup;
                return;
            }

            if (_finder == null)//after recompiling and remaining opened window
            {
                _finder = new MenuItemsFinder();
                _finder.Load();
            }
            
            _finder.Prefs.SearchString =
                GUILayoutCollection.SearchTextField(_finder.Prefs.SearchString, GUILayout.MinWidth(200));

            if (!_finder.Prefs.SearchString.Equals(_finder.Prefs.PreviousSearchString))
            {
                _finder.Prefs.PreviousSearchString = _finder.Prefs.SearchString;
                var key = _finder.Prefs.PreviousSearchString.ToLower();
                _finder.SelectedItems =
                    _finder.MenuItems.FindAll(m => m.SearchingKey.Contains(key));
                ;
            }

            GUILayout.BeginHorizontal();

            _finder.Prefs.OnlyWithValidate = GUILayout.Toggle(_finder.Prefs.OnlyWithValidate, "=OnlyWithValidate",
                GUILayout.MinWidth(100));

            if (GUILayout.Button("All Unstarred", GUILayout.MaxWidth(100)))
            {
                _finder.SelectedItems.ForEach(i =>
                {
                    if (i.Starred)
                    {
                        ToggleStarred(i);
                    }
                });
            }

            GUILayout.EndHorizontal();

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, true);

            if (_finder.Prefs.StarredMenuItems.Count > 0)
            {
                foreach (var item in _finder.MenuItems)
                {
                    if (!item.Starred)
                        continue;

                    if (_finder.Prefs.OnlyWithValidate && !item.HasValidate)
                    {
                        continue;
                    }

                    Draw(item, Color.green);
                }
            }

            if (_finder.SelectedItems.Count > 0)
            {
                foreach (var item in _finder.SelectedItems)
                {
                    if (item.Starred)
                        continue;

                    if (_finder.Prefs.OnlyWithValidate && !item.HasValidate)
                    {
                        continue;
                    }

                    Draw(item, Color.white);
                }
            }

            GUILayout.EndScrollView();
        }


        private void Draw(MenuItemLink item, Color defaultColor)
        {
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
                ToggleStarred(item);
            }

            GUILayout.EndHorizontal();
        }

        private void ToggleStarred(MenuItemLink item)
        {
            item.Starred = !item.Starred;

            if (item.Starred && !_finder.Prefs.StarredMenuItems.Contains(item.MenuItemPath))
            {
                _finder.Prefs.StarredMenuItems.Add(item.MenuItemPath);
            }
            else if (_finder.Prefs.StarredMenuItems.Contains(item.MenuItemPath))
            {
                _finder.Prefs.StarredMenuItems.Remove(item.MenuItemPath);
            }
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
    }
}