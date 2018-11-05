using System;
using SKTools.Base.Editor;
using UnityEditor;
using UnityEngine;

namespace SKTools.MenuItemsFinder
{
    internal partial class MenuItemsFinder
    {
        private string _itemFocusControl;
        private Vector2 _scrollPosition;
        private MenuItemLink _selectedMenuItem;

        private bool _wasItemsRemoving;

        private string FilterMenuItems
        {
            get { return _prefs.FilterString; }
            set
            {
                _prefs.FilterString = value;

                if (_prefs.FilterString != _prefs.PreviousFilterString)
                {
                    var key = !string.IsNullOrEmpty(_prefs.FilterString) ? _prefs.FilterString.ToLower() : string.Empty;
                    _prefs.PreviousFilterString = _prefs.FilterString = key;

                    SetFilteredItems(key);
                }
            }
        }

        private void SetFilteredItems(string key)
        {
            foreach (var item in _menuItems)
                item.IsFiltered = string.IsNullOrEmpty(key) ||
                                  !string.IsNullOrEmpty(item.Key) && item.Key.Contains(key) ||
                                  !string.IsNullOrEmpty(item.CustomName) &&
                                  item.CustomName.ToLower().Contains(key);
        }

        private void CleanRemovedItems()
        {
            if (_wasItemsRemoving)
            {
                _wasItemsRemoving = false;
                _menuItems = _menuItems.FindAll(i => !i.IsRemoved);
            }
        }

        private void DrawUnvailableState(Rect position)
        {
            //pivot = new Vector2(position.xMin + position.width * 0.5f, position.yMin + position.height * 0.5f);
            //var matrixBackup = GUI.matrix;
            //GUIUtility.RotateAroundPivot(angle%360, pivot);
            var width = _targetGui.Assets.LoadingImage.width;
            var height = _targetGui.Assets.LoadingImage.height;
            var rect = new Rect(position.width * 0.5f - width * 0.5f, position.height * 0.5f - height * 0.5f, width,
                height);
            GUI.DrawTexture(rect, _targetGui.Assets.LoadingImage);
            //GUI.matrix = matrixBackup; 
        }

        private void DrawSearchBar()
        {
            FilterMenuItems = DrawSearchBar(FilterMenuItems, _itemFocusControl == null);
        }

        private void DrawMenuBar()
        {
            _prefs.ShowMenuBar = EditorGUILayout.Foldout(_prefs.ShowMenuBar, "Menu");
            if (_prefs.ShowMenuBar)
            {
                GUILayout.BeginHorizontal();

                _prefs.HideUnityItems = GUILayout.Toggle(_prefs.HideUnityItems, "Hide Unity Items",
                    GUILayout.MaxWidth(120), GUILayout.MinWidth(120));

                _prefs.ShowOnlyStarred = GUILayout.Toggle(_prefs.ShowOnlyStarred, "Show Only Starred",
                    GUILayout.MaxWidth(120), GUILayout.MinWidth(120));

                _prefs.HideAllMissed = GUILayout.Toggle(_prefs.HideAllMissed, "Hide Missed Items",
                    GUILayout.MaxWidth(120), GUILayout.MinWidth(120));

                GUILayout.EndHorizontal();
            }
        }

        private void DrawItems()
        {
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, true);

            _menuItems.FindAll(m => m.Starred && !m.IsMissed && (!_prefs.HideUnityItems || !m.IsUnityMenu))
                .ForEach(DrawItem);

            if (!_prefs.ShowOnlyStarred)
            {
                if (!_prefs.HideAllMissed) _menuItems.FindAll(m => m.IsMissed).ForEach(DrawItem);

                _menuItems.FindAll(m =>
                        m.IsFiltered && !m.Starred && !m.IsMissed && (!_prefs.HideUnityItems || !m.IsUnityMenu))
                    .ForEach(DrawItem);
            }

            GUILayout.EndScrollView();

            CleanRemovedItems();
        }

        private void DrawItem(MenuItemLink item)
        {
            GUILayout.BeginHorizontal();

            var texture = item.Starred ? _targetGui.Assets.StarredImage : _targetGui.Assets.UnstarredImage;
            if (GUILayout.Button(texture, GUILayout.MaxWidth(24), GUILayout.MaxHeight(24)))
                item.Starred = !item.Starred;

            var defaultColor = item.Starred ? Color.green : Color.white;
            var label = item.Label;

            bool? validated = null;
            try
            {
                validated = item.CanExecute();
            }
            catch
            {
                validated = false;
            }

            if (validated == false) defaultColor = item.IsMissed ? Color.red : Color.gray;


            if (!item.IsEditName && !item.IsEditHotkey)
            {
                var previousColor = GUI.color;
                GUI.color = defaultColor;
                if (GUILayout.Button(label, _targetGui.Assets.MenuItemButtonStyle))
                    try
                    {
                        if (validated == false)
                        {
                            if (!item.IsMissed)
                            {
                                EditorUtility.DisplayDialog("Validation fail!",
                                    "Cant validate this option\n" + item.Label,
                                    "ok");
                            }
                            else
                            {
                                if (EditorUtility.DisplayDialog("Missing item!",
                                    "Do you want to remove this item?\n" + item.Label,
                                    "ok", "cancel"))
                                    ClickButton_Remove(item);
                            }

                            return;
                        }

                        item.Execute();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("cant execute this menu item: " + item + "\n" + ex);
                    }

                GUI.color = previousColor;
            }
            else
            {
                if (item.IsEditName)
                {
                    GUI.SetNextControlName("itemFocusControl");
                    item.EditName = GUILayout.TextField(item.EditName);

                    if (item.HotKey == null)
                    {
                        if (GUILayout.Button("+HotKey", GUILayout.MinWidth(90), GUILayout.MaxWidth(90)))
                        {
                            item.IsEditHotkey = true;
                            item.IsEditName = false;
                        }
                    }
                    else
                    {
                        GUILayout.Label(item.HotKey.Formatted, GUILayout.MinWidth(90), GUILayout.MaxWidth(90));
                    }
                }

                else if (item.IsEditHotkey)
                {
                    if (GUILayout.Button("Check&Add", GUILayout.MinWidth(90), GUILayout.MaxWidth(90)))
                    {
                        var error = "";
                        if (!TryAddHotKeyToItem(_selectedMenuItem, null, out error))
                            EditorUtility.DisplayDialog("Something went wrong!", error, "Try again!");
                    }

                    item.EditHotKey.Key = GUILayout.TextField(item.EditHotKey.Key);
                    GUILayout.Label(" Key");

                    item.EditHotKey.Alt = GUILayout.Toggle(item.EditHotKey.Alt, " Alt");

                    item.EditHotKey.Shift = GUILayout.Toggle(item.EditHotKey.Shift, " Shift");

                    item.EditHotKey.Cmd = GUILayout.Toggle(item.EditHotKey.Cmd, " Cmd");

                    if (GUILayout.Button("X", GUILayout.MinWidth(45), GUILayout.MaxWidth(45)))
                    {
                        item.IsEditHotkey = false;
                        item.IsEditName = true;
                        _itemFocusControl = "itemFocusControl";
                        GUI.FocusControl(_itemFocusControl);
                    }
                }
            }

            if (GUILayout.Button(_targetGui.Assets.SettingsImage, GUILayout.MaxWidth(24), GUILayout.MaxHeight(24)))
            {
                item.IsEditName = !item.IsEditName;
                item.IsEditHotkey = false;

                if (item.IsEditName)
                {
                    item.EditName = item.OriginalName;

                    _itemFocusControl = "itemFocusControl";
                    GUI.FocusControl(_itemFocusControl);
                }
                else
                {
                    if (item.EditName != item.OriginalName && !string.IsNullOrEmpty(item.EditName))
                    {
                        var ok = EditorUtility.DisplayDialog("You changed the current name of this menuItem",
                           string.Format("Do you want to change the previous name={0} on this new={1}?", item.OriginalName, item.EditName), "ok", "cancel");
                        if (ok)
                        {
                            item.CustomName = item.EditName;
                            UpdateLabel(item);
                        }
                    }
                    _itemFocusControl = null;
                }
            }

            if (GUILayout.Button("Open script", GUILayout.MinWidth(80), GUILayout.MaxWidth(80)))
            {
                var error = "";
                var filepath = "";

                FindScriptWhichContainsMenuItem(item, out filepath, out error);
                if (!string.IsNullOrEmpty(filepath))
                {
                    Utility.OpenFile(filepath);
                    EditorGUIUtility.systemCopyBuffer = item.OriginalPath;
                }

                if (!string.IsNullOrEmpty(error))
                {
                    var ok = EditorUtility.DisplayDialog("Can't open file that contains this menuItem",
                        "There happens tgis=" + error + "\n Do ypu want to open location of assembly?", "ok", "cancel");
                    if (ok) OpenAssemblyLocationThatContainsMenuItem(item);
                }
            }


            GUILayout.EndHorizontal();
        }

        private void ClickButton_Remove(MenuItemLink item)
        {
            _wasItemsRemoving = true;
            item.IsRemoved = true;
        }
    }
}