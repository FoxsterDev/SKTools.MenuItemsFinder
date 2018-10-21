using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace SKTools.Module.Base
{
    internal class AssetsProvider
    {
        public Dictionary<string, Object> AssetsDict { get; private set; }

        public AssetsProvider()
        {
            
        }
        public AssetsProvider(string directoryPath)
        {
            
        }

        public Object this[string nameAsset]
        {
            get { return GetAsset<Object>(nameAsset); }
        }
        
        public Type GetAsset<Type>(string name) where Type : UnityEngine.Object
        {
            if (!AssetsDict.ContainsKey(name))
            {
                Debug.LogAssertion("Cant get an asset with name=" + name);
            }

            return (Type) AssetsDict[name];
        }
        
        /// <summary>
        /// Just return new StackTrace(true). it will be used for detecting filePath of YourEditorWindow
        /// And it uses for loading assets relevant for the window
        /// </summary>
        /// <returns></returns>
        // protected abstract StackTrace GetStackTrace();
        private void LoadAssets()
        {
            AssetsDict = new Dictionary<string, Object>();
            try
            {
                var stackTrace = new StackTrace(true); //GetStackTrace();
                var assetPath = Path.Combine(new FileInfo(stackTrace.GetFrames()[0].GetFileName()).DirectoryName,
                    "Window Resources/");

                var files = Directory.GetFiles(assetPath, "*.*", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    if (Path.GetExtension(file) == ".meta")
                        continue;
                    assetPath = file.Substring(Application.dataPath.Length - "Assets".Length);
                    var asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                    AssetsDict[asset.name] = asset;
                }
            }
            catch
            {
                
            }
        }
    }
}