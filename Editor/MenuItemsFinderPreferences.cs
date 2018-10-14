using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace SKTools.MenuItemsFinder
{
    [Serializable]
    internal class MenuItemsFinderPreferences
    {
        private static MenuItemsFinderPreferences _current;
        public List<MenuItemLink> CustomizedMenuItems = new List<MenuItemLink>();

        public string FilterString = string.Empty;
        [NonSerialized] public bool HideAllMissed;

        [NonSerialized] public string PreviousFilterString = null;

        private static string GetFilePath
        {
            get
            {
                var stackTrace = new StackTrace(true);
                var editorDirectory = stackTrace.GetFrames()[0].GetFileName()
                    .Replace(typeof(MenuItemsFinderPreferences).Name + ".cs", string.Empty);

                return string.Concat(editorDirectory, "Prefs.json");
            }
        }

        public static MenuItemsFinderPreferences Current
        {
            get
            {
                if (_current != null)
                    return _current;
                _current = new MenuItemsFinderPreferences();
                if (File.Exists(GetFilePath))
                    EditorJsonUtility.FromJsonOverwrite(File.ReadAllText(GetFilePath), _current);
                else
                    Debug.LogError("Can't load prefs.json by path=" + GetFilePath);

                return _current;
            }
        }
    }
}