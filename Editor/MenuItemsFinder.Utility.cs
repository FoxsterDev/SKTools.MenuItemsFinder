using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace SKTools.MenuItemsFinder
{
    internal partial class MenuItemsFinder
    {
        /// <summary>
        /// I added this method because the standart GetWindow immediately create and show window, and I cant configure it before opening
        /// </summary>
        /// <param name="createIfNotExist">In some cases I need to check of exisiting already opened window</param>
        /// <typeparam name="T">Some type of editor window</typeparam>
        /// <returns>Return a window of type T</returns>
        private static T GetWindow<T>(bool createIfNotExist) where T : EditorWindow
        {
            var objectsOfTypeAll = Resources.FindObjectsOfTypeAll(typeof(T));
            if (objectsOfTypeAll.Length < 1)
            {
                if (!createIfNotExist) return null;
                return ScriptableObject.CreateInstance<T>();
            }

            var window = (T) objectsOfTypeAll[0];
            return window;
        }
      
        /// <summary>
        /// it is worst by time (about 30 ms) than the current variant with 3-foreaches
        /// </summary>
        /// <returns></returns>
        private IEnumerable<MethodInfo> GetAllStaticMethods()
        {
            var methods =(from  assembly in AppDomain.CurrentDomain.GetAssemblies()
                from type in assembly.GetTypes()
                from method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                select method).Distinct();

            return methods;
        }
        
        private T LoadAsset<T>(string assetDirectory, string assetName) where T : UnityEngine.Object
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