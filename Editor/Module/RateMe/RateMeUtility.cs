using System.Diagnostics;
using System.IO;

namespace SKTools.RateMeWindow
{
    internal class Utility
    {
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
    }
}