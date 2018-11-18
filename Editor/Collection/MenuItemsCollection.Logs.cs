#if UNITY_EDITOR_OSX
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using SKTools.Base.Editor;
using UnityEditor;
using UnityEngine.EventSystems;

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
                
                ///Users/sergeykha/Projects/FoxsterDev/SKToolsUnity/Assets/SKTools/MenuItemsFinder/Editor/term.sh
                return;
                UnityEngine.Debug.Log(sdkRoot);
                /*var output = "ps aux".Bash();
                UnityEngine.Debug.Log(output);
                return;*/
                /*ShellHelper.ShellRequest req = ShellHelper.ProcessCommand("ls",sdkRoot);
                req.onLog += delegate(int logType, string log) {
                    UnityEngine.Debug.Log(log);
                }; 
                return;*/
                ProcessStartInfo startInfo2 = new ProcessStartInfo("bash");
                startInfo2.FileName = @"/Applications/Utilities/Terminal.app/Contents/MacOS/Terminal";
                startInfo2.WorkingDirectory = sdkRoot;//"/";
                startInfo2.UseShellExecute = false;
                startInfo2.RedirectStandardInput = true;
                startInfo2.RedirectStandardOutput = false;
                startInfo2.CreateNoWindow = true;
                startInfo2.WindowStyle = System.Diagnostics.ProcessWindowStyle.Maximized;// .Normal;

                //startInfo2.StandardOutputEncoding = System.Text.UTF8Encoding.UTF8;
                //startInfo2.StandardErrorEncoding = System.Text.UTF8Encoding.UTF8;
                
                Process process = new Process();
                process.StartInfo = startInfo2;
                process.Start();

                process.StandardInput.AutoFlush = true;
                process.StandardInput.WriteLine("cd "+sdkRoot);
                process.StandardInput.WriteLine("echo helloworld222");
                //process.StandardInput.WriteLine("exit"); // if no exit then WaitForExit will lockup your program
                process.StandardInput.Flush();
                ; 

                //string line2 = process.StandardOutputs.ReadLine();
//UnityEngine.Debug.Log(line2);
                //process.WaitForExit();
            return;

            var startInfo = new ProcessStartInfo{
                    FileName = @"/Applications/Utilities/Terminal.app/Contents/MacOS/Terminal",
                    //Arguments = "ls",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal
                };
#if UNITY_EDITOR_OSX
                string splitChar = ":";
                startInfo.Arguments = "-c";
#elif UNITY_EDITOR_WIN
				string splitChar = ";";
				startInfo.Arguments = "/c";
				#endif
                var cmd = "cd "+sdkRoot;
                startInfo.Arguments += (" \"" + cmd + " \"");
                var uploadProc = Process.Start(startInfo);//(startInfo);
                
                //uploadProc.Start();
                //uploadProc.be
            }
        }
    }
}
#endif
