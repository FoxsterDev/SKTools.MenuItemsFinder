using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace SKTools.MenuItemsFinder
{
    internal partial class MenuItemsFinder
    {
        private T LoadAssetAtPath<T>(string assetDirectory, string assetName) where T : UnityEngine.Object
        {
            var assetPath = string.Concat(assetDirectory, assetName);
            var asset = (T) AssetDatabase.LoadAssetAtPath(assetPath, typeof (T));
            Assert.IsNotNull(asset, "Cant load asset, please check path=" +assetPath);
            return asset;
        }
        
        private void OpenFile(string filePath)
        {
#if !UNITY_EDITOR_WIN
            filePath = "file://" + filePath.Replace(@"\", "/");
#else
            filePath = @"file:\\" + filePath.Replace("/", @"\");;
#endif
            Application.OpenURL(filePath);
        }
    }
}