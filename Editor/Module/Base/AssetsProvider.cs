using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace SKTools.Module.Base
{
    internal class AssetsProvider
    {
        private Dictionary<string, Object> AssetsDict { get; set; }

        public AssetsProvider()
        {
            AssetsDict = new Dictionary<string, Object>();
        }
        
        public Object this[string nameAsset]
        {
            get { return GetAsset<Object>(nameAsset); }
        }
        
        public T GetAsset<T>(string name) where T : Object
        {
            if (!AssetsDict.ContainsKey(name))
            {
                Debug.LogAssertion("Cant get an asset with name=" + name);
            }

            return (T) AssetsDict[name];
        }

        public void LoadAssets(string absoluteDirectoryPath)
        {
            
            try
            {
                var files = Directory.GetFiles(absoluteDirectoryPath, "*.*", SearchOption.AllDirectories);
                foreach (var filePath in files)
                {
                    if (Path.GetExtension(filePath) == ".meta")
                        continue;
                    var assetPath = filePath.Substring(Application.dataPath.Length - "Assets".Length);
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