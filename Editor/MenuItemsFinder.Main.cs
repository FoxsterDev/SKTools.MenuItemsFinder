using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using SKTools.Base.Editor;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace SKTools.MenuItemsFinder
{
    internal partial class MenuItemsFinder 
    {
        private static MenuItemsFinder _instance;

        private static MenuItemsFinder GetFinder()
        {
            return _instance ?? (_instance = new MenuItemsFinder());
        }
        
        private readonly Preferences _prefs;
        private bool _isLoaded;
        private List<MenuItemLink> _menuItems;

        private MenuItemsFinder()
        {
            _prefs = new Preferences();
            _prefs.Load();
        }
        
        private bool IsCustomized(MenuItemLink item)
        {
            return item.Starred || !string.IsNullOrEmpty(item.CustomName) || item.IsMissed || item.CustomHotKeys.Count > 0;
        }

        private void LoadMenuItems()
        {
            try
            {
                _menuItems = GetMenuItems(_prefs.CustomizedMenuItems);
                _isLoaded = true;
            }
            catch (Exception ex)
            {
                Debug.LogError("[MenuItemsFinder] could not be loaded!");
            }
        }

        private void SavePrefs()
        {
            try
            {
                _prefs.CustomizedMenuItems = _menuItems.FindAll(IsCustomized).ToList();
                _prefs.CustomizedMenuItems.ForEach(item =>
                {
                    if (item.CustomHotKeys.Count > 0) item.CustomHotKeys.RemoveAll(h => !h.IsVerified);
                });

                _prefs.Save();
            }
            catch
            {
            }
        }
        
        private List<MenuItemLink> GetMenuItems(List<MenuItemLink> customizedItems)
        {
            var menuItemData = LoadMenuItemData();
            var menuItemsLinksDict = new Dictionary<string, MenuItemLink>(menuItemData.Count);
            var menuItemLinks = CreateMenuItemLinks(menuItemData, menuItemsLinksDict);
           
            CustomizeMenuItems(menuItemLinks, customizedItems, menuItemsLinksDict);
            
            menuItemLinks.ForEach(UpdateLabel);
            menuItemLinks.Sort((left, right) => left.Path[0] - right.Path[0]);
            
            return menuItemLinks;
        }

        private void UpdateLabel(MenuItemLink item)
        {
            item.UpdateLabel();
            item.UpdateLabelWithHotKeys();
        }
        
        private void CustomizeMenuItems(List<MenuItemLink> menuItems, List<MenuItemLink> customizedItems,  Dictionary<string, MenuItemLink> menuItemLinksDict)
        {
            MenuItemLink menuItem;
            foreach (var customizedItem in customizedItems)
            {
                if (string.IsNullOrEmpty(customizedItem.Path))
                    continue;

                menuItemLinksDict.TryGetValue(customizedItem.Path, out menuItem);
                
                if (menuItem == null)
                {
                    menuItems.Add(customizedItem);
                    continue;
                }

                menuItem.UpdateFrom(customizedItem);
                menuItem.UpdateCustomHotKeysFrom(customizedItem);
            }
        }

        private List<MenuItemLink> CreateMenuItemLinks(Dictionary<string, MenuItemData> menuItemData, Dictionary<string, MenuItemLink> menuItemLinksDict)
        {
            var menuItems = new List<MenuItemLink>(menuItemData.Count);
            
            foreach (var entry in menuItemData)
            {
                if (entry.Value.TargetMethodValidate != null && entry.Value.TargetMethod == null)
                {
                    Debug.LogWarning("There is a validate method without execution method=" +
                                     entry.Value.TargetMethodValidate.Name + " menupath=" +
                                     entry.Value.TargetAttributeValidate.menuItem);
                    continue;
                }

                var item = new MenuItemLink(entry.Value);
                item.UpdateOriginalHotKey();
                
                menuItems.Add(item);
                menuItemLinksDict[item.Path] = item;
            }

            return menuItems;
        }
        
        private Dictionary<string, MenuItemData> LoadMenuItemData()
        {
            var dict = new Dictionary<string, MenuItemData>(200);
            
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes();
                
                foreach (var type in types)
                {
                    var methods = type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

                    foreach (var method in methods)
                    {
                        ExtractMenuItem(method, dict);
                    }
                }
            }

            return dict;
        }

        private void ExtractMenuItem(MethodInfo method, Dictionary<string, MenuItemData> dict)
        {
            var items = method.GetCustomAttributes(typeof(MenuItem), false).Cast<MenuItem>().ToArray();
            if (items.Length != 1)
            {
                return;
            }

            var key = items[0].menuItem;

            MenuItemData data;
            dict.TryGetValue(key, out data);
            if (data == null)
            {
                data = new MenuItemData();
                dict.Add(key, data);
            }

            if (items[0].validate)
            {
                data.TargetAttributeValidate = items[0];
                data.TargetMethodValidate = method;
            }
            else
            {
                data.TargetAttribute = items[0];
                data.TargetMethod = method;
            }
        }
        
        private void OpenAssemblyLocationThatContainsMenuItem(MenuItemLink item)
        {
            var directoryPath = new FileInfo(item.DeclaringType.Assembly.Location).DirectoryName;
            SKTools.Base.Editor.Utility.OpenFile(directoryPath);
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