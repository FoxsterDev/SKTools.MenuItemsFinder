using System;
using SKTools.Base.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace SKTools.MenuItemsFinder
{
    internal partial class MenuItemsFinder
    {
        partial void CustomHotKeysEditable();
        partial void DrawMenuItemHotKeys();
          
        
        private MenuItemLink _selectedMenuItem;
        private Vector2 _scrollPosition;

        private bool _wasItemsRemoving;

        private string FilterMenuItems
        {
            get { return _prefs.FilterString; }
            set
            {
                _prefs.FilterString = value;
                
                if (!_prefs.FilterString.Equals(_prefs.PreviousFilterString))
                {
                    var key = _prefs.FilterString.ToLower();
                    _prefs.PreviousFilterString = _prefs.FilterString;

                    foreach (var item in _menuItems)
                    {
                        item.IsFiltered = string.IsNullOrEmpty(key) ||
                                          item.Key.Contains(key) ||
                                          (!string.IsNullOrEmpty(item.CustomName) &&
                                           item.CustomName.ToLower().Contains(key));
                    }
                }
            }
        }
        
        private void OnWindowLostFocus(Rect position)
        {
            SavePrefs();
        }

        private void OnWindowClosed(Rect position)
        {
            SavePrefs();
        }

        private void OnWindowGui(Rect position)
        {
            if (!_isLoaded || EditorApplication.isCompiling)
            {
                DrawUnvailableState(position);
                return;
            }

            /*if (!_isLoadedWindowStyles)
            {
                _isLoadedWindowStyles = true;
                LoadWindowStyles();
            }*/

            DrawSearchBar();
            DrawMenuBar();
            DrawItems();
            DrawSupportBar();
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
            FilterMenuItems = GUILayoutCollection.SearchTextField(FilterMenuItems, focusControl, GUILayout.MinWidth(200));
        }

        private void DrawMenuBar()
        {
            GUILayout.BeginHorizontal();

            if (!_prefs.HideAllMissed && _menuItems.Find(m => m.IsMissed) != null)
            {
                if (GUILayout.Button("Hide All Missed", GUILayout.MaxWidth(100)))
                {
                    _prefs.HideAllMissed = true;
                }
            }

            GUILayout.EndHorizontal();
        }

        private void DrawItems()
        {
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, true);

            _menuItems.FindAll(m => m.Starred && !m.IsMissed).ForEach(DrawNormalState);

            if (!_prefs.HideAllMissed)
            {
                _menuItems.FindAll(m => m.IsMissed).ForEach(DrawMissedState);
            }

            _menuItems.FindAll(m => m.IsFiltered && !m.Starred && !m.IsMissed).ForEach(DrawNormalState);
            
            GUILayout.EndScrollView();
            
            CleanRemovedItems();
        }

        private void DrawMissedState(MenuItemLink item)
        {
            Assert.IsTrue(item.IsMissed);
            
            GUILayout.BeginHorizontal();
            
            if (GUILayout.Button(string.Concat("<color=red>", "[Missed]", "</color>") + item.Label, _targetGui.Assets.MenuItemButtonStyle))
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

            GUILayout.Label("Set name:", GUILayout.MinWidth(80), GUILayout.MaxWidth(80));

            GUI.SetNextControlName("RolledOutMenuItemCustomName");

            _selectedMenuItem.CustomNameEditable = GUILayout.TextField(_selectedMenuItem.CustomNameEditable,
                GUILayout.MinWidth(150), GUILayout.MaxWidth(150));

            if (GUILayout.Button("+", GUILayout.MinWidth(24), GUILayout.MaxWidth(24)))
            {
                ClickButton_SetCustomName(item);
            }

            GUILayout.EndHorizontal();

            _selectedMenuItem.ShowNotice = EditorGUILayout.Foldout(_selectedMenuItem.ShowNotice, "Add Notice");
            if (_selectedMenuItem.ShowNotice)
            {
                _selectedMenuItem.Notice = GUILayout.TextArea(_selectedMenuItem.Notice, GUILayout.MinHeight(60));
            }

            DrawMenuItemHotKeys();
        }

        private void ClickButton_SetCustomName(MenuItemLink link)
        {
            if (_selectedMenuItem != null && !string.IsNullOrEmpty(_selectedMenuItem.CustomNameEditable))
            {
                link.CustomName = _selectedMenuItem.CustomNameEditable;
                link.UpdateLabel();
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
                OpenFile(fullPath);
                EditorGUIUtility.systemCopyBuffer = item.Path;
            }
        }

        private void DrawSupportBar()
        {
            GUILayoutCollection.SupportFooterBar(
                MenuItemsFinderVersion.Version.ToString(),
                MenuItemsFinderVersion.ReadmeUrl, 
                MenuItemsFinderVersion.ReadmeUrl, 
                MenuItemsFinderVersion.AskQuestionUrlInSkype);
        }
    }
}