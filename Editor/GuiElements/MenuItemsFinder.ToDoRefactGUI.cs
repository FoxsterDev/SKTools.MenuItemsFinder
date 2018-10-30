using System;
using System.Linq;
using SKTools.Base.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace SKTools.MenuItemsFinder
{
    internal partial class MenuItemsFinder
    {
        private MenuItemLink _selectedMenuItem;
        private Vector2 _scrollPosition;

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
            //Debug.LogError("SetFilteredItems: "+ key +" count: "+ _menuItems.Count);
            foreach (var item in _menuItems)
            {
                item.IsFiltered = string.IsNullOrEmpty(key) ||
                                  (!string.IsNullOrEmpty(item.Key) && item.Key.Contains(key)) ||
                                  (!string.IsNullOrEmpty(item.CustomName) &&
                                   item.CustomName.ToLower().Contains(key));
            }
        }
      
        
        private void CleanRemovedItems()
        {
            if (_wasItemsRemoving)
            {
                _wasItemsRemoving = false;
                _menuItems = _menuItems.FindAll(i => !i.IsRemoved);
            }
        }
            
        private void SelectItem(MenuItemLink item)
        {
            _selectedMenuItem = item;
            _selectedMenuItem.CustomNameEditable = _selectedMenuItem.CustomName;
            CustomHotKeysEditable();
        }

        private void UnSelectItem()
        {
            _selectedMenuItem.CustomHotKeys.RemoveAll(i => !i.IsVerified);
            _selectedMenuItem = null;
            _selectedMenuItemCustomHotKeysEditable = null;
        }
    
        private void DrawUnvailableState(Rect position)
        {
            //pivot = new Vector2(position.xMin + position.width * 0.5f, position.yMin + position.height * 0.5f);
            //var matrixBackup = GUI.matrix;
            //GUIUtility.RotateAroundPivot(angle%360, pivot);
            var width = _targetGui.Assets.LoadingImage.width;
            var height = _targetGui.Assets.LoadingImage.height;
            var rect = new Rect(position.width * 0.5f - width * 0.5f, position.height * 0.5f - height * 0.5f, width, height);
            GUI.DrawTexture(rect, _targetGui.Assets.LoadingImage);
            //GUI.matrix = matrixBackup; 
        }

        private void DrawSearchBar()
        {
            var focusControl = _selectedMenuItem == null;
            FilterMenuItems = DrawSearchBar(FilterMenuItems, focusControl);
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

            _menuItems.FindAll(m => m.Starred && !m.IsMissed && (!_prefs.HideUnityItems || !m.IsUnityMenu)).ForEach(DrawNormalState);

            if (!_prefs.ShowOnlyStarred)
            {
                if (!_prefs.HideAllMissed)
                {
                    _menuItems.FindAll(m => m.IsMissed).ForEach(DrawMissedState);
                }

                _menuItems.FindAll(m => m.IsFiltered && !m.Starred && !m.IsMissed && (!_prefs.HideUnityItems || !m.IsUnityMenu)).ForEach(DrawNormalState);
            }

            GUILayout.EndScrollView();
            
            CleanRemovedItems();
        }

        private void DrawMissedState(MenuItemLink item)
        {
            Assert.IsTrue(item.IsMissed);
            
            GUILayout.BeginHorizontal();
            
            if (GUILayout.Button(item.Label, _targetGui.Assets.MenuItemButtonStyle))
            {
                try
                {
                    EditorUtility.DisplayDialog("Missing!", "Cant execute this option\n" + item.Label, "ok");
                    return;
                }
                catch (Exception ex)
                {
                    Debug.LogError("[MenuItemsFinder] Cant execute this menu item: " + item + "\n" + ex);
                }
            }

            var previousColor = GUI.color;
            GUI.color = Color.red;

            if (GUILayout.Button("[-]", GUILayout.MaxWidth(24), GUILayout.MaxHeight(24)))
            {
                if (EditorUtility.DisplayDialog("Missing item!", "Do you want to remove this item?\n" + item.Label,
                    "ok", "cancel"))
                {
                    ClickButton_Remove(item);
                }
            }

            GUI.color = previousColor;
            
            GUILayout.EndHorizontal();
        }

        private void DrawNormalState(MenuItemLink item)
        {
            GUILayout.BeginHorizontal();

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

            if (validated == false) defaultColor = Color.gray;
            
            var previousColor = GUI.color;
            GUI.color = defaultColor;

            if (GUILayout.Button(label, _targetGui.Assets.MenuItemButtonStyle))
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

            var texture = (item.Starred ? _targetGui.Assets.StarredImage : _targetGui.Assets.UnstarredImage);
            if (GUILayout.Button(texture, GUILayout.MaxWidth(24), GUILayout.MaxHeight(24)))
            {
                ClickButton_ToggleStarred(item);
            }

            if (GUILayout.Button(_targetGui.Assets.SettingsImage, GUILayout.MaxWidth(24), GUILayout.MaxHeight(24)))
            {
                if (ClickButton_ToggleSettings(item))
                {
                    GUI.FocusControl("RolledOutMenuItemCustomName");
                }
            }
            
            GUILayout.EndHorizontal();
                      
            if (_selectedMenuItem != null && _selectedMenuItem.Key.Equals(item.Key))
            {
                DrawSettings(item);
            }
        }

        private void DrawSettings(MenuItemLink item)
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Open file", GUILayout.MinWidth(80), GUILayout.MaxWidth(80)))
            {
                var error = "";
                ClickButton_TryOpenFileThatContainsThisMenuItem(item, out error);
                if (!string.IsNullOrEmpty(error))
                {
                    var ok = EditorUtility.DisplayDialog("Can't open file that contains this menuItem",
                        "There happens tgis=" + error + "\n Do ypu want to open location of assembly?", "ok", "cancel");
                    if (ok)
                    {
                        OpenAssemblyLocationThatContainsMenuItem(item);
                    }
                }
            }

            GUILayout.Label("Custom name:", GUILayout.MinWidth(70), GUILayout.MaxWidth(70));

            GUI.SetNextControlName("RolledOutMenuItemCustomName");

            _selectedMenuItem.CustomNameEditable = GUILayout.TextField(_selectedMenuItem.CustomNameEditable,
                GUILayout.MinWidth(150), GUILayout.MaxWidth(150));

            if (GUILayout.Button("set", EditorStyles.miniButton, GUILayout.MinWidth(36), GUILayout.MaxWidth(36)))
            {
                ClickButton_SetCustomName(item);
            }
            
            GUILayout.Label("AssemblyName: " + item.AssemblyName, GUILayout.MinWidth(100));

            GUILayout.EndHorizontal();

            _selectedMenuItem.ShowNotice = EditorGUILayout.Foldout(_selectedMenuItem.ShowNotice, "Add Notice");
            if (_selectedMenuItem.ShowNotice)
            {
                _selectedMenuItem.Notice = GUILayout.TextArea(_selectedMenuItem.Notice, GUILayout.MinHeight(60));
            }

            DrawMenuItemHotKeys();
        }

        private void ClickButton_SetCustomName(MenuItemLink item)
        {
            if (_selectedMenuItem != null)
            {
                item.CustomName = _selectedMenuItem.CustomNameEditable;
                UpdateLabel(item);
            }
        }

        private void ClickButton_Remove(MenuItemLink item)
        {
            _wasItemsRemoving = true;
            item.IsRemoved = true;
        }
        
        private bool ClickButton_ToggleSettings(MenuItemLink item)
        {
            if (_selectedMenuItem == null)
            {
                SelectItem(item);
                return true;
            }

            var newSettings = _selectedMenuItem.Key != item.Key;
            UnSelectItem();

            if (newSettings)
            {
                SelectItem(item);
                return true;
            }

            return false;
        }

        private void ClickButton_ToggleStarred(MenuItemLink item)
        {
            item.Starred = !item.Starred;
        }

        private void ClickButton_TryOpenFileThatContainsThisMenuItem(MenuItemLink item, out string error)
        {
            var fullPath = FindScriptWhichContainsMenuItem(item, out error);
            if (!string.IsNullOrEmpty(fullPath))
            {
                Utility.OpenFile(fullPath);
                EditorGUIUtility.systemCopyBuffer = item.Path;
            }
        }
    }
}
