    using System.Linq;
using UnityEditor;
//using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleEnums;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace SKTools.MenuItemsFinder
{
    internal partial class MenuItemsFinder
    {
  
        private void OpenAssemblyLocationThatContainsMenuItem(MenuItemLink item)
        {
            var directoryPath = new FileInfo(item.DeclaringType.Assembly.Location).DirectoryName;
            OpenFile(directoryPath);
        }
        ///Users/sergeykha/Library/Unity/cache/packages/packages.unity.com/com.unity.textmeshpro@1.2.4/Scripts/Editor
        ///Users/sergeykha/Projects/Foxster/SKToolsUnity/Packages/manifest.json
        ///Unity.PackageManagerUI.Editor.dll
        private string FindScriptWhichContainsMenuItem(MenuItemLink item, out string error)
        {
            //typeof(UnityEditor.PackageManager.)
            /*
             * UnityEditor.PackageManager.UI
{
    internal class PackageCollection
             */
            
            /* public IEnumerable<PackageInfo> PackageInfos
        {
            get { return packageInfos; }
        }*/
            //UnityEditor.PackageManager.UI
            //UnityEditor.PackageManager.PackageCollection
            //PackageCollection
            //UnityEditor.PackageManager.PackageCollection .Client .PackageCollection
            error = "";


            //var k = File.ReadAllText("sers/sergeykha/Projects/Foxster/SKToolsUnity/Packages/manifest.json");
            
//            var l = EditorJsonUtility.ToJson(new Manifest{dependencies = new Dictionary<string, string> {{"lsall", "lsllas"}}});// .FromJsonOverwrite(k,);
  //          Debug.Log(l);
            //Unity.TextMeshPro.Editor
            var assemblyFilePath = Path.GetFileNameWithoutExtension(item.AssemlyFilePath);
            Debug.Log(assemblyFilePath);

            try
            {
                var assemblyCprojPath = Application.dataPath.Replace("Assets", assemblyFilePath + ".csproj");
                if (File.Exists(assemblyCprojPath))
                {
                    var lines = File.ReadAllLines(assemblyCprojPath);
                    var assemblyFiles = new List<string>();
                    var infoFound = false;

                    foreach (var line in lines)
                    {
                        if (line.Contains("<Compile Include="))
                        {
                            infoFound = true;
                            assemblyFiles.Add(line.Split('"')[1]);
                        }
                        else if (infoFound)
                        {
                            break;
                        }
                    }

                    var itemDeclaringTypeName = item.DeclaringType.Name;

                    foreach (var assetPath in assemblyFiles)
                    {
                        if (assetPath.Contains(itemDeclaringTypeName)
                        ) //suppose that type and file name equals about another cases need to implement
                        {
                            return Path.Combine(Application.dataPath, assetPath.Substring(7));
                        }
                    }
                    
                    error = "cant detect file from " + assemblyFilePath + ".dll in assets folder\n";
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            return null;
        }
    }
}