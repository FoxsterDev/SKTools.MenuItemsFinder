using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SKTools.MenuItemsFinder
{
    internal partial class MenuItemsFinder
    {
        private string FindScriptWhichContainsMenuItem(MenuItemLink item, out string error)
        {
            error = "";
            //Unity.TextMeshPro.Editor
            var assemblyFilePath = Path.GetFileNameWithoutExtension(item.AssemlyFilePath);
            Debug.Log(assemblyFilePath);

            try
            {
                var assemblyCprojPath = Application.dataPath.Replace("Assets", assemblyFilePath + ".csproj");
                if (!File.Exists(assemblyCprojPath))
                {
                    error = "cant detect file from " + assemblyFilePath + ".dll in assets folder\n";
                    return null;
                }

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
                    if (assetPath.Contains(itemDeclaringTypeName)) //suppose that type and file name equals about another cases need to implement
                    {
                        return Path.Combine(Application.dataPath, assetPath.Substring(7));
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            error = "cant detect file from " + assemblyFilePath + ".dll in assets folder\n";
            return null;
            ///Users/sergeykha/Library/Unity/cache/packages/packages.unity.com/com.unity.textmeshpro@1.2.4/Scripts/Editor
        }
    }
}