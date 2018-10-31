using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SKTools.MenuItemsFinder
{
    internal partial class MenuItemsFinder
    {
        private static Assembly packageManagerAssembly;
        private static Func<object> _hasFetchedPackageList;
        private static Func<object> _searchPackages;
        private static System.Type _packageCollection;
        private static object _packageCollectionInstance;
        private static Func<object, bool> _setFilter;
        ///Users/skhalandachev/Library/Unity/cache/packages/packages.unity.com/com.unity.package-manager-ui@1.9.11
        private  static void WaitFetchingPackages()
        {
            var packageInfos = _packageCollection.GetField("LastSearchPackages", BindingFlags.Instance | BindingFlags.NonPublic);
            var packages = packageInfos.GetValue(_packageCollectionInstance);
            if (packages != null)
            {
                EditorApplication.update -= WaitFetchingPackages;
                Debug.Log("was founded..");
               //public PackageInfo Current { get { return Versions.FirstOrDefault(package => package.IsCurrent); } }
        //private Dictionary<string, Package> Packages;
               
                Debug.Log(packages);
                var packageInfoType = packageManagerAssembly.GetType("UnityEditor.PackageManager.UI.PackageInfo");
                var packageInfoTargetType =
                    packageInfoType.GetProperty("Info", BindingFlags.Instance | BindingFlags.Public);
            
                foreach (var package in (IEnumerable) packages)
                {
                    var info = (UnityEditor.PackageManager.PackageInfo)packageInfoTargetType.GetValue(package, null);
                    Debug.Log(info.resolvedPath); //UnityEditor.PackageManager.UI.PackageInfo
                }
            }
        }
        
        public static void PrintProjectPackages()
        {
             packageManagerAssembly =  AppDomain.CurrentDomain.GetAssemblies().ToList().Find(a => a.GetName().Name == "Unity.PackageManagerUI.Editor");
            // return;
            _packageCollection = packageManagerAssembly.GetType("UnityEditor.PackageManager.UI.PackageCollection");
            _packageCollectionInstance = _packageCollection.GetField("instance", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            _hasFetchedPackageList = () =>
            {
                return _packageCollection
                    .GetMethod("HasFetchedPackageList", BindingFlags.Instance | BindingFlags.Public)
                    .Invoke(_packageCollectionInstance, null);
            };
            
            _searchPackages = () =>
            {
                return _packageCollection
                    .GetMethod("SearchPackages", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(_packageCollectionInstance, null);
            };
            
            var myEnum = packageManagerAssembly.GetType("UnityEditor.PackageManager.UI.PackageFilter");
            var all = Enum.GetValues(myEnum).GetValue(2);
            Debug.Log(all);
            
            _setFilter = (filter) =>
            {
                return (bool)_packageCollection
                    .GetMethod("SetFilter", BindingFlags.Instance | BindingFlags.Public)
                    .Invoke(_packageCollectionInstance, new object[]{filter, true});
            };

            //_setFilter(all);
            _searchPackages();
            if (EditorApplication.update != null) EditorApplication.update -= WaitFetchingPackages;
            EditorApplication.update += WaitFetchingPackages;
        }
    }
}