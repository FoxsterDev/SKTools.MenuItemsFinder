using System;
using System.Collections.Generic;

namespace SKTools.MenuItemsFinder
{
    [System.Serializable]
    internal class MenuItemHotKey : IEquatable<MenuItemHotKey>, IComparable<MenuItemHotKey>
    {
        public string Key;
        public bool Shift;
        public bool Alt;
        public bool Cmd;
        public bool IsVerified;
        
        [NonSerialized]
        public bool IsOriginal;
       
        public override string ToString()
        {
            return ToFormat(this);
        }
        
        public bool Equals(MenuItemHotKey other)
        {
            return other != null && Key == other.Key && Alt ==  other.Alt && Shift == other.Shift && Cmd == other.Cmd;//%#0
        }

        public int CompareTo(MenuItemHotKey other)
        {
            return string.Compare(this, other);
        }
        
        public static implicit operator string(MenuItemHotKey k)
        {
            return k != null ? k.ToString() : default(string);
        }

        public static string ToFormat(MenuItemHotKey hotkey)
        {
            var str = string.Empty;
            
            if (hotkey.Cmd)
            {
#if UNITY_EDITOR_OSX
                str = "cmd+";
#else
                str = "ctrl+";
               
#endif
            }

            if (hotkey.Alt)
            {
                str += "alt+";
            }

            if (hotkey.Shift)
            {
                str += "shift+";
            }

            str += hotkey.Key;
            return str;
        }

        public static string ToPack(MenuItemHotKey hotkey)
        {
            //% (ctrl on Windows, cmd on macOS), # (shift), & (alt)
            var str = "";
            if (hotkey.Cmd) str += "%";
            if (hotkey.Shift) str += "#";
            if (hotkey.Alt) str += "&";
            str += hotkey.Key;
            return str;
        }

        /*
        * To create a hotkey you can use the following special characters: % (ctrl on Windows, cmd on macOS), # (shift), & (alt). If no special modifier key combinations are required the key can be given after an underscore. For example to create a menu with hotkey shift-alt-g use "MyMenu/Do Something #&g".
         * To create a menu with hotkey g and no key modifiers pressed use "MyMenu/Do Something _g".
         Some special keyboard keys are supported as hotkeys, for example "#LEFT" would map to shift-left. 
         The keys supported like this are: LEFT, RIGHT, UP, DOWN, F1 .. F12, HOME, END, PGUP, PGDN.
         A hotkey text must be preceded with a space character ("MyMenu/Do_g" won't be interpreted as hotkey, while "MyMenu/Do _g" will).
        */
        public static void Extract(string itemPath, out int index, out string hotkeyString, out string key,
            out bool shift, out bool alt, out bool cmd)
        {
            key = hotkeyString = string.Empty;
            index = -1;
            shift = false;
            alt = false;
            cmd = false;
            if (string.IsNullOrEmpty(itemPath))
                return;

            var chars = itemPath.ToCharArray();
            var underScoreIndex = -1;
            var slashIndex = -1;
            var whiteSpaceIndex = -1;
            var keyChars = new Stack<char>();
            var hotkeyChars = new Stack<char>();
            var indecatorIndex = -1;
            var modifiersIndex = -1;

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

                    var modifier = false;
                    if (c == '#')
                    {
                        modifier = shift = true;
                    }

                    if (c == '%')
                    {
                        modifier = cmd = true;
                    }

                    if (c == '&')
                    {
                        modifier = alt = true;
                    }

                    if (modifier)
                    {
                        indecatorIndex = k;
                        if (modifiersIndex < 0)
                        {
                            modifiersIndex = k;
                        }
                    }
                    else
                    {
                        keyChars.Push(c);
                    }
                }
            }

            if (whiteSpaceIndex > -1 && slashIndex > -1 && whiteSpaceIndex > slashIndex)
            {
                if (underScoreIndex > -1)
                {
                    index = underScoreIndex + 1;
                    hotkeyString = new string(hotkeyChars.ToArray());
                    key = new string(keyChars.ToArray());
                    return;
                }

                if (indecatorIndex > -1)
                {
                    index = whiteSpaceIndex + 1;
                    hotkeyString = new string(hotkeyChars.ToArray());
                    key = new string(keyChars.ToArray());
                }
            }
        }
    }
}