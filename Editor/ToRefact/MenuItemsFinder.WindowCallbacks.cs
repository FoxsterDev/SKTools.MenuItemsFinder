﻿using UnityEditor;
using UnityEngine;

namespace SKTools.MenuItemsFinder
{
    internal partial class MenuItemsFinder
    {
        private static bool IsWindowOpen
        {
            get { return EditorPrefs.GetBool(typeof(MenuItemsFinder).Name, false); }
            set { EditorPrefs.SetBool(typeof(MenuItemsFinder).Name, value); }
        }

        [MenuItem("SKTools/MenuItems Finder #%m")]
        private static void ShowWindow()
        {
            var window = GetWindow(true);

            var finder = GetFinder();
            finder.Load();
            finder.SetWindow(window);

            window.Show();
            window.Focus();
            IsWindowOpen = true;
        }

        [InitializeOnLoadMethod]
        private static void MenuItemsFinderWindow_CheckReload()
        {
            if (!IsWindowOpen) return;
            
            var window = GetWindow(false);
            if (window == null)
            {
                IsWindowOpen = false;
                return;
            }

            var finder = GetFinder();
            finder.Load();
            finder.SetWindow(window);
            
            window.Focus();
        }

        private static MenuItemsFinderWindow GetWindow(bool createIfNotExist)
        {
            var objectsOfTypeAll = Resources.FindObjectsOfTypeAll(typeof(MenuItemsFinderWindow));
            if (objectsOfTypeAll.Length < 1)
            {
                if (!createIfNotExist) return null;
                return ScriptableObject.CreateInstance<MenuItemsFinderWindow>();
            }

            var window = (MenuItemsFinderWindow) objectsOfTypeAll[0];
            return window;
        }

        private void SetWindow(MenuItemsFinderWindow window)
        {
            LoadGUIAssets();
            
            window.autoRepaintOnSceneChange = true;
            window.titleContent = new GUIContent("MenuItems");
            window.DrawGuiCallback = OnWindowGui;
            window.CloseCallback = OnWindowClosed;
            window.LostFocusCallback = OnWindowLostFocus;
        }

        private void OnWindowLostFocus(MenuItemsFinderWindow window)
        {
            SavePrefs();
        }

        private void OnWindowClosed(MenuItemsFinderWindow window)
        {
            IsWindowOpen = false;
            SavePrefs();
            Dispose();
        }

        private void OnWindowGui(MenuItemsFinderWindow window)
        {
            if (!_isLoaded)
            {
                DrawUnvailableState(window);
                return;
            }
            
            if (EditorApplication.isCompiling)
            {
                DrawUnvailableState(window);
                return;
            }

            if (!_isCreatedStyles)
            {
                _isCreatedStyles = true;
                CreateStyles();
            }
            
            DrawSearchBar();
            DrawMenuBar();
            DrawItems();

            CleanRemovedItems();
            
            GUILayoutCollection.SupportFooterBar(MenuItemsFinderVersion.Version.ToString(), MenuItemsFinderVersion.ReadmeUrl, MenuItemsFinderVersion.ReadmeUrl, 
                MenuItemsFinderVersion.AskQuestionUrlInSkype);
        }
    }
}