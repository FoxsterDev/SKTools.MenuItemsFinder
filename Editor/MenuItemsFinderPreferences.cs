using System;
using System.Collections.Generic;
using System.IO;
using SKTools.Core.Editor;
using UnityEditor;
using UnityEngine;

namespace SKTools.MenuItemsFinder
{
    [Serializable]
    internal class Preferences
    {
        public List<MenuItemLink> CustomizedMenuItems = new List<MenuItemLink>();

        public string FilterString;
        public bool ShowOnlyStarred;
        public bool HideUnityItems;

        [NonSerialized]
        public bool HideAllMissed;

        [NonSerialized]
        public bool ShowMenuBar;

        [NonSerialized]
        public string PreviousFilterString;

        public void Load()
        {
            try
            {
                var filePath = Utility.GetPathRelativeToCurrentDirectory("Editor Resources", "Prefs.json");
                if (File.Exists(filePath))
                {
                    EditorJsonUtility.FromJsonOverwrite(File.ReadAllText(filePath), this);
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public void Save()
        {
            try
            {
                var filePath = Utility.GetPathRelativeToCurrentDirectory("Editor Resources", "Prefs.json");
                File.WriteAllText(filePath, EditorJsonUtility.ToJson(this, true));
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }
}
