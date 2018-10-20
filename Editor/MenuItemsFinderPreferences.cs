using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SKTools.MenuItemsFinder
{
    [Serializable]
    internal class MenuItemsFinderPreferences
    {
        public List<MenuItemLink> CustomizedMenuItems = new List<MenuItemLink>();

        public string FilterString = string.Empty;

        [NonSerialized] public bool HideAllMissed;
        [NonSerialized] public string PreviousFilterString = null;

        private string GetFilePath
        {
            get { return string.Concat(DirectoryPath, "Prefs.json"); }
        }

        public string Error { get; private set; }
        /// <summary>
        /// It uses stacjtrace detect a place of the code in Assets
        /// </summary>
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
                return DirectoryPath.Replace("Editor", "Editor Resources")
                    .Substring(Application.dataPath.Length - "Assets".Length);
            }
        }

        public void Load()
        {
            try
            {
                var filePath = GetFilePath;
                if (File.Exists(filePath))
                    EditorJsonUtility.FromJsonOverwrite(File.ReadAllText(filePath), this);
            }
            catch (Exception e)
            {
                Error = e.Message;
            }
        }

        public void Save()
        {
            try
            {
                File.WriteAllText(GetFilePath, EditorJsonUtility.ToJson(this, true));
            }
            catch (Exception e)
            {
                Error = e.Message;
            }
        }
    }
}