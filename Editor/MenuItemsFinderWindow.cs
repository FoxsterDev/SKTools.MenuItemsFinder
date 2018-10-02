using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace SKTools.MenuItemsFinder
{
    //[Done] windows doesn work starred + hotkeys ctrl + shift..
    //[Done] replace special hotkeys to readable symbols
    //[Done] to separate searchbox
    //to plan: check validate method
    //to provide and check context for menu item
    //[Done] to add star prefs
    //[Done] add json prefs
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
            _finder.Prefs.SearchString =
                GUILayoutCollection.SearchTextField(_finder.Prefs.SearchString, GUILayout.MinWidth(200));

            if (!_finder.Prefs.SearchString.Equals(_finder.Prefs.PreviousSearchString))
            {
                _finder.Prefs.PreviousSearchString = _finder.Prefs.SearchString;
                 var key = _finder.Prefs.PreviousSearchString.ToLower();
                _finder.SelectedItems = _finder.MenuItems.FindAll(m => m.SearchingKey.Contains(key));// && !m.MenuItem.validate);
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

        private void OnDestroy()
        {
            _finder.SavePrefs();
            AssetDatabase.Refresh();
        }

        private void Draw(MenuItemLink item, Color color)
        {
            GUILayout.BeginHorizontal();
            var previousColor = GUI.color;
            GUI.color = color;
            if (GUILayout.Button(item.Label, _menuItemButtonStyle, GUILayout.MaxWidth(300)))
            {
                Debug.Log("Try execute menuItem=" + item);
                try
                {
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
    }
}