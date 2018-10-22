using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace SKTools.MenuItemsFinder
{
    internal class Utility
    {
        /// <summary>
        /// In development mode I want to measure time of some methods to avoid abuse compilation other systems
        /// </summary>
        /// <param name="method">Method what should be runned</param>
        public static void DiagnosticRun(Action method)
        {
            
#if !FOXSTER_DEV_MODE
            method();
#else
            var watch = new Stopwatch();
            watch.Start();

            method();

            watch.Stop();
            Debug.LogFormat("DiagnosticRun {0} takes={1}", method.Method.Name, watch.ElapsedMilliseconds + "ms");
#endif
        }

        public static string GetPath(string subName)
        {
            return Path.Combine(GetDirectory(), subName);
        }
        /// <summary>
        /// Get directory of place where was called this method, simple way to detect places scripts. To avoid hardcoded pathes
        /// </summary>
        /// <returns></returns>
        public static string GetDirectory()
        {
            return new FileInfo(new StackTrace(true).GetFrames()[0].GetFileName()).DirectoryName;
        }

        public static void OpenFile(string filePath)
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