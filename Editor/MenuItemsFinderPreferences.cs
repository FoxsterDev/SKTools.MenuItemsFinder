using System;
using System.Collections.Generic;
using System.IO;
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

        public void Load()
        {
            try
            {
                var filePath = Utility.GetPath("Prefs.json");
                if (File.Exists(filePath))
                    EditorJsonUtility.FromJsonOverwrite(File.ReadAllText(filePath), this);
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
                File.WriteAllText(Utility.GetPath("Prefs.json"), EditorJsonUtility.ToJson(this, true));
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }
}