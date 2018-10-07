﻿using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace SKTools.MenuItemsFinder
{
    internal class MenuItemsTests
    {
        private static readonly char[] hotKeysIndicators = new []
        {
            '%', '#', '_', '&'
        };
        
       
        internal static void FindHotKey(string itemPath, out int index, out string hotkey)
        {
            hotkey = string.Empty;
            index = -1;
            var chars = itemPath.ToCharArray();
            var underScoreIndex = -1;
            var slashIndex = -1;
            var whiteSpaceIndex = -1;
            var chars_ = new Stack<char>();
            var indecatorIndex = -1;
            if (chars[chars.Length - 1] != ' ')
            {
                for (int k = chars.Length - 1; k > -1; k--)
                {
                    var c = chars[k];
                    if (c == '/')
                    {
                        slashIndex = k;
                        break;
                    }
                    
                    if (whiteSpaceIndex > -1)
                    {
                        continue;
                    }
                    
                    if (c == ' ')
                    {
                        whiteSpaceIndex = k;
                        continue;
                    }

                    if (c == '_')
                    {
                        underScoreIndex = k;
                        continue;
                    }

                    chars_.Push(c);
                    if (c == '%' || c == '#' || c == '&')
                    {
                        indecatorIndex = k;
                    }
                }
            }
            /*
                    * To create a hotkey you can use the following special characters: % (ctrl on Windows, cmd on macOS), # (shift), & (alt). If no special modifier key combinations are required the key can be given after an underscore. For example to create a menu with hotkey shift-alt-g use "MyMenu/Do Something #&g". To create a menu with hotkey g and no key modifiers pressed use "MyMenu/Do Something _g".
                     Some special keyboard keys are supported as hotkeys, for example "#LEFT" would map to shift-left. The keys supported like this are: LEFT, RIGHT, UP, DOWN, F1 .. F12, HOME, END, PGUP, PGDN.
                     A hotkey text must be preceded with a space character ("MyMenu/Do_g" won't be interpreted as hotkey, while "MyMenu/Do _g" will).
                    */
            if (whiteSpaceIndex > -1)
            {
                if (underScoreIndex > -1)
                {
                    index = underScoreIndex + 1;
                    hotkey = new string(chars_.ToArray());
                    return;
                }
            }
        }
        
        [Test]
        public void HotKeyExtractFromItemPath()
        {
            var index = -1;
            var hotkey = "";
            FindHotKey("Window/Analysis/Profiler %&7", out index, out hotkey);
        }

    }
}

/*index = itemPath.LastIndexOf(' ');
         if (index > -1)
         {
             hotkey = itemPath.Substring(index);
             //
             if (hotkey.Contains('%') || hotkey.Contains('#') || hotkey.Contains('&'))
             {
                 hotkey =
#if UNITY_EDITOR_OSX
                     hotkey.Replace("%", "cmd+").
#else
                 hotkey.Replace("%", "ctrl+").
 #endif
                     
                         Replace("#", "shift+").Replace("&", "alt+");
                 hotkey = string.Concat("<color=cyan>", hotkey, "</color>");
             }
         }*/