using System;
using System.IO;
using SKTools.Base.Editor;
using UnityEditor;
using UnityEngine;

namespace SKTools.RateMeWindow
{
    [System.Serializable]
    public class RateMeConfig
    {
        public byte MinStar = 3;
        public byte MaxStar = 5;
        
        public string RequestLabel = "How do you like it?";
        public string RateButtonText = "Rate";
        
        public string AssetStoreUrl = "https://assetstore.unity.com/packages/tools/utilities/monkey-editor-commands-productivity-booster-119938";
        public string GithubUrl = "https://github.com/FoxsterDev/SKTools.MenuItemsFinder";
        public string GithubMessage = "Do you have a github account?";
        public string RateGithubButtonText = "Rate me on Github";
        public string RateAssetStoreButtonText = "Rate me on AssetStore";

        public string FeedbackMessage = "Please enter your message here";
        public string FeedbackTitle = "FEEDBACK/SUGGESTION";

        public string EmailAddress = "s.khalandachev@gmail.com";
        public string EmailSubject = "[Support] SKTools.MenuItemsFinder FEEDBACK/SUGGESTION";
        public string EmailAdditionalInfoFormat = "\n\n\n\n________\n\nPlease Do Not Modify This\n\n{0}\n\n________\n";
        
        public RateMeConfig Load()
        {
            try
            {
                var filePath = Utility.GetPath("Editor Resources", "RateMeConfig.json");
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
            return new RateMeConfig();
        }

        public void Save()
        {
            try
            {
                var filePath = Utility.GetPath("Editor Resources", "RateMeConfig.json");
                File.WriteAllText(filePath, EditorJsonUtility.ToJson(this, true));
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }
}