using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SKTools.MenuItemsFinder
{
    internal partial class MenuItemsFinder
    {
        private List<string> GetCSharpScriptsPathesFromAssemblyCProjectFile(string assemblyCprojPath)
        {
            var assemblyFilesPathes = new List<string>();
            if (File.Exists(assemblyCprojPath))
            {
                var lines = File.ReadAllLines(assemblyCprojPath);
                var infoFound = false;

                foreach (var line in lines)
                {
                    if (line.Contains("<Compile Include="))
                    {
                        infoFound = true;
                        assemblyFilesPathes.Add(line.Split('"')[1]);
                    }
                    else if (infoFound)
                    {
                        break;
                    }
                }
            }

            return assemblyFilesPathes;
        }

        private void FindScriptWhichContainsMenuItem(
            MenuItemLink item, out string resultFilePath,
            out string resultError)
        {
            resultError = "";
            resultFilePath = "";
            var itemDeclaringTypeName = item.DeclaringType.Name;
            var assemblyFilePath = Path.GetFileNameWithoutExtension(item.AssemlyFilePath);

            try
            {
                var assemblyCprojPath = Application.dataPath.Replace("Assets", assemblyFilePath + ".csproj");
                var assemblyFilesPathes = GetCSharpScriptsPathesFromAssemblyCProjectFile(assemblyCprojPath);

                if (assemblyFilesPathes.Count > 0)
                {
                    foreach (var assetPath in assemblyFilesPathes)
                    {
                        if (assetPath.Contains(itemDeclaringTypeName)
                        ) //easy suppose that type and file name equals about another cases need to implement
                        {
                            resultFilePath = Path.Combine(Application.dataPath, assetPath.Substring(7));
                            return;
                        }
                    }

                    foreach (var assetPath in assemblyFilesPathes)
                    {
                        resultFilePath = Path.Combine(Application.dataPath, assetPath.Substring(7));

                        if (File.ReadAllText(resultFilePath).Contains(itemDeclaringTypeName)
                        ) //easy suppose that type and file name equals about another cases need to implement
                        {
                            return;
                        }
                    }
                }
                else
                {
#if UNITY_2018_1_OR_NEWER

                    Debug.Log("Check Unity Packages");
                    //todo fix it on windows
                    var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library");
                    var target = Path.Combine(path, "Unity/cache/packages/packages.unity.com");

                    var directory = new DirectoryInfo(target);
                    var files = directory.GetFiles("*.cs", SearchOption.AllDirectories)
                                         .Where(p => p.FullName.Contains("Editor"))
                                         .Select(p => p.FullName).ToList();

                    foreach (var filePath in files)
                    {
                        if (filePath.Contains(itemDeclaringTypeName)
                        ) //easy suppose that type and file name equals about another cases need to implement
                        {
                            resultFilePath = filePath;
                            return;
                        }
                    }

                    foreach (var filePath in files)
                    {
                        if (File.ReadAllText(filePath).Contains(itemDeclaringTypeName)
                        ) //easy suppose that type and file name equals about another cases need to implement
                        {
                            resultFilePath = filePath;
                            return;
                        }
                    }
#endif
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            resultError = "cant detect file from " + assemblyFilePath + ".dll in assets folder\n";
        }

#if UNITY_2018_1_OR_NEWER

        private Assembly _packageManagerAssembly;
        private Func<object> _searchPackagesMathod;
        private Type _packageCollection;
        private object _packageCollectionInstance;

        private void WaitFetchingPackages()
        {
            var lastSearchPackagesField =
                _packageCollection.GetField("LastSearchPackages", BindingFlags.Instance | BindingFlags.NonPublic);
            var lastSearchPackages = lastSearchPackagesField.GetValue(_packageCollectionInstance);

            if (lastSearchPackages != null)
            {
                EditorApplication.update -= WaitFetchingPackages;

                Debug.Log("was founded..");

                var addPackageInfosMethod =
                    _packageCollection.GetMethod("AddPackageInfos", BindingFlags.Instance | BindingFlags.NonPublic);
                addPackageInfosMethod.Invoke(_packageCollectionInstance, new object[1] { lastSearchPackages });

                var packagesDictionaryField =
                    _packageCollection.GetField("Packages", BindingFlags.Instance | BindingFlags.NonPublic);
                var packagesDictionary = packagesDictionaryField.GetValue(_packageCollectionInstance);

                var packageInfoType = _packageManagerAssembly.GetType("UnityEditor.PackageManager.UI.PackageInfo");
                var packageInfoTargetType =
                    packageInfoType.GetProperty("Info", BindingFlags.Instance | BindingFlags.Public);

                var packageType = _packageManagerAssembly.GetType("UnityEditor.PackageManager.UI.Package");
                var versionToDisplayProperty =
                    packageType.GetProperty("VersionToDisplay", BindingFlags.Instance | BindingFlags.NonPublic);

                var packagesPathes = new List<string>();
                foreach (DictionaryEntry entry in (IDictionary) packagesDictionary)
                {
                    var packageInfo = versionToDisplayProperty.GetValue(entry.Value, null);
                    if (packageInfo == null)
                    {
                        continue;
                    }

                    var target =
                        (UnityEditor.PackageManager.PackageInfo) packageInfoTargetType.GetValue(packageInfo, null);
                    if (!string.IsNullOrEmpty(target.resolvedPath))
                    {
                        if (!target.resolvedPath.Contains(EditorApplication.applicationContentsPath))
                        {
                            packagesPathes.Add(target.resolvedPath);
                        }
                    }
                }

                packagesPathes.ForEach(Debug.Log);
            }
        }

        private void FetchingPackages()
        {
            _packageManagerAssembly = AppDomain.CurrentDomain.GetAssemblies().ToList()
                                               .Find(a => a.GetName().Name == "Unity.PackageManagerUI.Editor");
            _packageCollection = _packageManagerAssembly.GetType("UnityEditor.PackageManager.UI.PackageCollection");
            _packageCollectionInstance = _packageCollection
                                         .GetField("instance", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);

            _searchPackagesMathod = () => _packageCollection
                                          .GetMethod("SearchPackages", BindingFlags.Instance | BindingFlags.NonPublic)
                                          .Invoke(_packageCollectionInstance, null);

            _searchPackagesMathod();

            if (EditorApplication.update != null)
            {
                EditorApplication.update -= WaitFetchingPackages;
            }

            EditorApplication.update += WaitFetchingPackages;
        }
#endif
    }
}
