using System;
using System.Collections.Generic;
using System.IO;
using SKTools.Base.Editor;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace SKTools.MenuItemsFinder
{
    [Serializable]
    internal class Preferences
    {
        public List<MenuItemLink> CustomizedMenuItems = new List<MenuItemLink>();

        public string FilterString = string.Empty;

        [NonSerialized] public bool HideAllMissed;
        [NonSerialized] public string PreviousFilterString = null;

        public Preferences Load()
        {
            try
            {
                var filePath = Utility.GetPathRelativeToExecutableCurrentFile("Editor Resources", "Prefs.json");
                if (File.Exists(filePath))
                {
                    EditorJsonUtility.FromJsonOverwrite(File.ReadAllText(filePath), this);
                    return this;
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            return new Preferences();
        }

        public void Save()
        {
            try
            {
                var filePath = Utility.GetPathRelativeToExecutableCurrentFile("Editor Resources", "Prefs.json");
                File.WriteAllText(filePath, EditorJsonUtility.ToJson(this, true));
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }
}