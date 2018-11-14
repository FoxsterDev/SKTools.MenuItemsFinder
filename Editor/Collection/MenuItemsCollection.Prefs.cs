#if UNITY_EDITOR_OSX
using System;
using System.IO;
using SKTools.Base.Editor;
using UnityEditor;

namespace SKTools.MenuItemsFinder
{
    internal partial class MenuItemsCollection
    {
        private static string EditorPrefsFilePath
        {
            get
            {
                string path;
#if UNITY_EDITOR_OSX
                path = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                    "Library/Preferences/com.unity3d.UnityEditor5.x.plist");
#elif UNITY_EDITOR_WIN
On Windows, EditorPrefs are stored in the registry under the HKCU\Software\Unity Technologies\UnityEditor N.x key where N.x 
    is the major version number.
#endif
                return path;
            }
        }

        [MenuItem(MenuAssetPath + "Prefs/Open Editor Prefs", false, Priority)]
        public static void OpenEditorPrefs()
        {
            Utility.OpenFile(EditorPrefsFilePath);
        }

        [MenuItem(MenuAssetPath + "Prefs/Open Editor Prefs", true, Priority)]
        public static bool OpenEditorPrefsValidate()
        {
            return File.Exists(EditorPrefsFilePath);
        }

    }
}
#endif
