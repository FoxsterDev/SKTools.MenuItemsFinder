using System;
using System.Collections.Generic;

namespace SKTools.MenuItemsFinder
{
    //[]
    public class MenuItemHotKey : IEquatable<MenuItemHotKey>
    {
        public string Value;
        public string Formatted;
        public int StartIndex;
        
        public string Key;
        public bool Shift;
        public bool Alt;
        public bool Cmd;
        public bool IsVerified;
        
        public MenuItemHotKey()
        {
            
        }
        
        public MenuItemHotKey(string menuItemPath)
        {
            Formatted = string.Empty;
            Extract(menuItemPath, out StartIndex, out Value);

            if (StartIndex > -1)
            {
                Formatted = ToFormat(Value);
            }
        }
        
        public override string ToString()
        {
            return Formatted;
        }

        public static implicit operator string(MenuItemHotKey k)
        {
            return k.ToString();
        }
        
        private static string ToFormat(string hotkey)
        {
            var formatted =
#if UNITY_EDITOR_OSX
                hotkey.Replace("%", "cmd+").
#else
                hotkey.Replace("%", "ctrl+").
 #endif
                    Replace("#", "shift+").Replace("&", "alt+");

            formatted = string.Concat("<color=cyan>", formatted, "</color>");
            return formatted;
        }
        
        /*
        * To create a hotkey you can use the following special characters: % (ctrl on Windows, cmd on macOS), # (shift), & (alt). If no special modifier key combinations are required the key can be given after an underscore. For example to create a menu with hotkey shift-alt-g use "MyMenu/Do Something #&g".
         * To create a menu with hotkey g and no key modifiers pressed use "MyMenu/Do Something _g".
         Some special keyboard keys are supported as hotkeys, for example "#LEFT" would map to shift-left. 
         The keys supported like this are: LEFT, RIGHT, UP, DOWN, F1 .. F12, HOME, END, PGUP, PGDN.
         A hotkey text must be preceded with a space character ("MyMenu/Do_g" won't be interpreted as hotkey, while "MyMenu/Do _g" will).
        */
        internal static void Extract(string itemPath, out int index, out string hotkey)
        {
            hotkey = string.Empty;
            index = -1;
            if (string.IsNullOrEmpty(itemPath))
                return;

            var chars = itemPath.ToCharArray();
            var underScoreIndex = -1;
            var slashIndex = -1;
            var whiteSpaceIndex = -1;
            var hotkeyChars = new Stack<char>();
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

                    hotkeyChars.Push(c);
                    if (c == '%' || c == '#' || c == '&')
                    {
                        indecatorIndex = k;
                    }
                }
            }

            if (whiteSpaceIndex > -1 && slashIndex > -1 && whiteSpaceIndex > slashIndex)
            {
                if (underScoreIndex > -1)
                {
                    index = underScoreIndex + 1;
                    hotkey = new string(hotkeyChars.ToArray());
                    return;
                }

                if (indecatorIndex > -1)
                {
                    index = whiteSpaceIndex + 1;
                    hotkey = new string(hotkeyChars.ToArray());
                }
            }
        }

        public bool Equals(MenuItemHotKey other)
        {
            return string.Equals(Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is MenuItemHotKey && Equals((MenuItemHotKey) obj);
        }

        public override int GetHashCode()
        {
            return (Value != null ? Value.GetHashCode() : 0);
        }
    }
}