using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
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

        private string GetFilePath
        {
            get
            {
               
                return string.Concat(DirectoryPath, "Prefs.json");
            }
        }

        private string DirectoryPath
        {
            get
            {
                var stackTrace = new StackTrace(true);
                return stackTrace.GetFrames()[0].GetFileName()
                    .Replace(typeof(MenuItemsFinderPreferences).Name + ".cs", string.Empty);
            }
        }
        
        public string GetDirectoryAssetsPath
        {
            get
            {
                return DirectoryPath.Replace("Editor", "Editor Resources").Substring(Application.dataPath.Length - "Assets".Length);
            }
        }
        
        public static MenuItemsFinderPreferences Current
        {
            get
            {
                if (_current != null)
                    return _current;
                
                _current = new MenuItemsFinderPreferences();
                
                var filePath = _current.GetFilePath;
                if (File.Exists(filePath))
                    EditorJsonUtility.FromJsonOverwrite(File.ReadAllText(filePath), _current);
                else
                    Debug.LogError("Can't load prefs.json by path=" + filePath);

                return _current;
            }
        }

        public void Save()
        {
            try
            {
                _current = null;
                File.WriteAllText(GetFilePath, EditorJsonUtility.ToJson(this, true));
            }
            catch
            {
            }
        }
    }
}