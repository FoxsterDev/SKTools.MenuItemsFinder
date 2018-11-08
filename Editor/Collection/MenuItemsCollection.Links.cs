﻿using UnityEditor;
using UnityEngine;

namespace SKTools.MenuItemsFinder
{
    internal partial class MenuItemsCollection
    {
        [MenuItem(MenuAssetPath + "Links/Platform dependent compilation", false, Priority + 2)]
        public static void OpenPlatformDependentCompilation()
        {
            Application.OpenURL(@"https://docs.unity3d.com/Manual/PlatformDependentCompilation.html");
        }
        
        [MenuItem(MenuAssetPath + "Links/Log Files", false, Priority + 2)]
        public static void OpenLogFiles()
        {
            Application.OpenURL(@"https://docs.unity3d.com/Manual/LogFiles.html");
        }
   
    }
}