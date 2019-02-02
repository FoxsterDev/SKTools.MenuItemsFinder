using UnityEditor;
using UnityEngine;

namespace SKTools.MenuItemsFinder
{
    internal partial class MenuItemsCollection
    {
        [MenuItem(MenuAssetPath + "Links/Unity/Platform dependent compilation", false, Priority)]
        public static void OpenPlatformDependentCompilation()
        {
            Application.OpenURL(@"https://docs.unity3d.com/Manual/PlatformDependentCompilation.html");
        }

        [MenuItem(MenuAssetPath + "Links/Unity/Log Files", false, Priority)]
        public static void OpenLogFiles()
        {
            Application.OpenURL(@"https://docs.unity3d.com/Manual/LogFiles.html");
        }

        [MenuItem(MenuAssetPath + "Links/Unity/Hotkeys", false, Priority)]

        public static void OpenUnityHotkeys()
        {
            Application.OpenURL(@"https://docs.unity3d.com/Manual/UnityHotkeys.html");
        }
    }
}
