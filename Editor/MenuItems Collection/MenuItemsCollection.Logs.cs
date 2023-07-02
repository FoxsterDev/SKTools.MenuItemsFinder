#if UNITY_EDITOR_OSX
using System;
using System.Diagnostics;
using System.IO;
using SKTools.Editor;
using UnityEditor;

namespace SKTools.MenuItemsFinder
{
    internal partial class MenuItemsCollection
    {
#if !FOXSTER_DEV_MODE
        private const string MenuAssetPath = "Assets/SKTools/Collection/";
#else
        private const string MenuAssetPath = "SKTools/";
#endif
        private const int Priority = 2000;

        private static string EditorLogFilePath
        {
            get
            {
                string path;
#if UNITY_EDITOR_OSX
                path = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                    "Library/Logs/Unity/Editor.log");
#elif UNITY_EDITOR_WIN
//	C:\Users\username\AppData\Local\Unity\Editor\Editor.log
            path =
 Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) , @"Unity\Editor\Editor.log"));
#endif
                return path;
            }
        }

        private static string PlayerLogFilePath
        {
            get
            {
                var path = string.Empty;
#if UNITY_EDITOR_OSX
                path = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                    "Library/Logs/Unity/Player.log");
#elif UNITY_EDITOR_WIN
//C:\Users\username\AppData\LocalLow\CompanyName\ProductName\output_log.txt
            path = string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Low\",
                Application.companyName, @"\", Application.productName, @"\output_log.txt");
#else
            throw new Exception("can't support now ~/.config/unity3d/CompanyName/ProductName/Player.log");
#endif
                return path;
            }
        }

        [MenuItem(MenuAssetPath + "Logs/Open Editor Log", false, Priority)]
        public static void OpenEditorLog()
        {
            Utility.OpenFile(EditorLogFilePath);
        }

        [MenuItem(MenuAssetPath + "Logs/Open Editor Log", true, Priority)]
        public static bool OpenEditorLogValidate()
        {
            return File.Exists(EditorLogFilePath);
        }

        [MenuItem(MenuAssetPath + "Logs/Open Player Log", false, Priority)]
        public static void OpenPlayerLog()
        {
            Utility.OpenFile(PlayerLogFilePath);
        }

        [MenuItem(MenuAssetPath + "Logs/Open Player Log", true, Priority)]
        public static bool OpenPlayerLogValidate()
        {
            return File.Exists(PlayerLogFilePath);
        }

        [MenuItem(MenuAssetPath + "Logs/Open Logcat", false, Priority)]
        public static void OpenLogcatConsole()
        {
            var sdkRoot = EditorPrefs.GetString("AndroidSdkRoot");
            if (!string.IsNullOrEmpty(sdkRoot))
            {
                ProcessStartInfo startInfo3 = new ProcessStartInfo("bash");
                startInfo3.WorkingDirectory = "Users/sergeykha/Projects/FoxsterDev/SKToolsUnity/Assets/SKTools/MenuItemsFinder/Editor/";
                startInfo3.UseShellExecute = true;
                startInfo3.Arguments = "term.sh ls";
                startInfo3.CreateNoWindow = true;
                //startInfo3.FileName = "term";
                Process process2 = new Process();
                process2.StartInfo = startInfo3;
                process2.Start();
            }
        }
    }
}
#endif