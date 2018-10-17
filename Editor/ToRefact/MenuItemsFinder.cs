﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Assertions;
using Debug = UnityEngine.Debug;

namespace SKTools.MenuItemsFinder
{
    public delegate void FinderDelegate();
    public delegate void FinderDelegate<T>(T obj);

    internal partial class MenuItemsFinder
    {
        partial void CustomHotKeysEditable();
        
        private void SelectItem(MenuItemLink item)
        {
            _selectedMenuItem = item;
            _selectedMenuItem.CustomNameEditable = _selectedMenuItem.CustomName;
            CustomHotKeysEditable();
        }
        private MenuItemLink _selectedMenuItem;
        private Vector2 _scrollPosition;

        private GUIStyle _menuItemButtonStyle,
            _unstarredMenuItemButtonStyle,
            _starredMenuItemButtonStyle,
            _settingsMenuItemButtonStyle;
        
        public Texture2D StarredImage, UnstarredImage, LoadingImage, SettingsImage;


        private bool _wasRemoving;
        private bool _isLoaded;
        private List<MenuItemLink> _menuItems;

        private static MenuItemsFinder _instance;
        private readonly MenuItemsFinderPreferences _prefs;
        
        private static MenuItemsFinder GetFinder()
        {
            return _instance ?? new MenuItemsFinder();
        }
       
        private MenuItemsFinder()
        {
            _prefs = new MenuItemsFinderPreferences();
            _prefs.Load();
        }

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
        
        private void Load()
        {
            try
            {
                _menuItems = GetAllMenuItems(_prefs.CustomizedMenuItems);
                _isLoaded = true;
            }
            catch (Exception ex)
            {
                Debug.LogError("[MenuItemsFinder] could not be loaded!");
            }
        }

        private void SavePrefs()
        {
            try
            {
                var customizedItems = _menuItems.FindAll(i => i.IsCustomized).ToList();
                customizedItems.ForEach(item=> { if (item.CustomHotKeys.Count > 0)
                {
                    item.CustomHotKeys.RemoveAll(h => !h.IsVerified);
                }});
                
                _prefs.CustomizedMenuItems = new List<MenuItemLink>(customizedItems);
                _prefs.Save();
            }
            catch
            {
            }
        }
        
        private void CleanRemovedItems()
        {
            if (_wasRemoving)
            {
                _wasRemoving = false;
                _menuItems = _menuItems.FindAll(i => !i.IsRemoved);
            }
        }
        
        private void UnSelectItem()
        {
            _selectedMenuItem.CustomHotKeys.RemoveAll(i => !i.IsVerified);
            _selectedMenuItem = null;
            _selectedMenuItemCustomHotKeysEditable = null;
        }

    
        private void LoadGUIAssets()
        {
            var assetsPath = _prefs.GetDirectoryAssetsPath;
                
            UnstarredImage = LoadAssetAtPath<Texture2D>(assetsPath, "unstarred.png");
            StarredImage =   LoadAssetAtPath<Texture2D>(assetsPath, "starred.png");
            LoadingImage =   LoadAssetAtPath<Texture2D>(assetsPath,"loading.png");
            SettingsImage =  LoadAssetAtPath<Texture2D>(assetsPath,"settings.png");
        }
               
        partial void DrawMenuItemHotKeys();

        public void Dispose()
        {
            Resources.UnloadAsset(StarredImage);
            Resources.UnloadAsset(UnstarredImage);
            Resources.UnloadAsset(LoadingImage);
            Resources.UnloadAsset(SettingsImage);
        }
     
        private bool _isCreatedStyles = false;

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
                        _unstarredMenuItemButtonStyle.normal.background = UnstarredImage;

            _starredMenuItemButtonStyle = new GUIStyle(_unstarredMenuItemButtonStyle);

            _starredMenuItemButtonStyle.active.background =
                _starredMenuItemButtonStyle.focused.background =
                    _starredMenuItemButtonStyle.hover.background =
                        _starredMenuItemButtonStyle.normal.background = StarredImage;

            _settingsMenuItemButtonStyle = new GUIStyle(_unstarredMenuItemButtonStyle);
            _settingsMenuItemButtonStyle.overflow = new RectOffset();
            _settingsMenuItemButtonStyle.active.background =
                _settingsMenuItemButtonStyle.focused.background =
                    _settingsMenuItemButtonStyle.hover.background =
                        _settingsMenuItemButtonStyle.normal.background = SettingsImage;
        }

        
        private void DrawUnvailableState(MenuItemsFinderWindow window)
        {
            //pivot = new Vector2(position.xMin + position.width * 0.5f, position.yMin + position.height * 0.5f);
            //var matrixBackup = GUI.matrix;
            //GUIUtility.RotateAroundPivot(angle%360, pivot);
            var width = LoadingImage.width;
            var height = LoadingImage.height;
            var rect = new Rect(window.position.width * 0.5f - width * 0.5f, window.position.height * 0.5f - height * 0.5f, width, height);
            GUI.DrawTexture(rect, LoadingImage);
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
        }

        private void DrawMissedState(MenuItemLink item)
        {
            Assert.IsTrue(item.IsMissed);
            
            GUILayout.BeginHorizontal();
            
            if (GUILayout.Button(string.Concat("<color=red>", "[Missed]", "</color>") + item.Label, _menuItemButtonStyle))
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

            if (GUILayout.Button(label, _menuItemButtonStyle))
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

            var texture = (item.Starred ? StarredImage : UnstarredImage);
            if (GUILayout.Button(texture, GUILayout.MaxWidth(24), GUILayout.MaxHeight(24)))
            {
               ClickButton_ToggleStarred(item);
            }

            if (GUILayout.Button(SettingsImage, GUILayout.MaxWidth(24), GUILayout.MaxHeight(24)))
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
            _wasRemoving = true;
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
        private void OpenAssemblyLocationThatContainsMenuItem(MenuItemLink item)
        {
            var directoryPath = new FileInfo(item.DeclaringType.Assembly.Location).DirectoryName;
            OpenFile(directoryPath);
        }

        private T LoadAssetAtPath<T>(string assetDirectory, string assetName) where T : UnityEngine.Object
        {
            var assetPath = string.Concat(assetDirectory, assetName);
            var asset = (T) AssetDatabase.LoadAssetAtPath(assetPath, typeof (T));
            Assert.IsNotNull(asset, "Cant load asset, please check path=" +assetPath);
            return asset;
        }
        
        private void OpenFile(string filePath)
        {
#if !UNITY_EDITOR_WIN
            filePath = "file://" + filePath.Replace(@"\", "/");
#else
            filePath = @"file:\\" + filePath.Replace("/", @"\");;
#endif
            Application.OpenURL(filePath);
        }
    }
}