using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;

namespace SKTools.MenuItemsFinder
{
    [Serializable]
    internal class Preferences
    {
        public List<MenuItemLink> CustomizedMenuItems = new List<MenuItemLink>();

        public string FilterString = string.Empty;

        [NonSerialized] public bool HideAllMissed;
        [NonSerialized] public string PreviousFilterString = null;

        public string Error { get; private set; }
        
        public void Load()
        {
            try
            {
                var filePath = Utility.GetPath("Prefs.json");
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
                File.WriteAllText(Utility.GetPath("Prefs.json"), EditorJsonUtility.ToJson(this, true));
            }
            catch (Exception e)
            {
                Error = e.Message;
            }
        }
    }
}