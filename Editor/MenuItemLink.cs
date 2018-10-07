using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SKTools.MenuItemsFinder
{
    internal class MenuItemLink
    {
        private readonly MenuItemData _menuItem;
        public readonly string Label;
        public readonly string Key;
        public readonly string HotKey;
        public readonly string HotKeyFormatted;

        public bool Starred;
        public string CustomName;

        public string MenuItemPath
        {
            get { return _menuItem.TargetAttribute.menuItem; }
        }

        public bool HasValidate
        {
            get { return _menuItem.TargetMethodValidate != null; }
        }

        public MenuItemLink(MenuItemData menuItem)
        {
            _menuItem = menuItem;
            Label = menuItem.TargetAttribute.menuItem;
            var hotkeyStartIndex = -1;
            FindHotKey(Label, out hotkeyStartIndex, out HotKey);
            
            if (hotkeyStartIndex > -1)
            {
                HotKeyFormatted = ToFormatHotKey(HotKey);
                Label = Label.Substring(0, hotkeyStartIndex);
            }
        
            Key = Label.ToLower(); //without hotkey
            Label = string.Concat(Label, HotKeyFormatted);
        }

        public bool CanExecute()
        {
            if (HasValidate)
            {
                return (bool) _menuItem.TargetMethodValidate.Invoke(null, null);
            }

            return true;
        }

        public void Execute()
        {
            _menuItem.TargetMethod.Invoke(null, null);
        }

        public override string ToString()
        {
            return string.Format("MenuItem={0}; Method={1}", Label, _menuItem.TargetMethod);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MenuItemLink) obj);
        }

        public override int GetHashCode()
        {
            return (_menuItem != null ? _menuItem.GetHashCode() : 0);
        }

        private static string ToFormatHotKey(string hotkey)
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

        internal static void FindHotKey(string itemPath, out int index, out string hotkey)
        {
            hotkey = string.Empty;
            index = -1;
            if(string.IsNullOrEmpty(itemPath))
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

            /*
            * To create a hotkey you can use the following special characters: % (ctrl on Windows, cmd on macOS), # (shift), & (alt). If no special modifier key combinations are required the key can be given after an underscore. For example to create a menu with hotkey shift-alt-g use "MyMenu/Do Something #&g".
             * To create a menu with hotkey g and no key modifiers pressed use "MyMenu/Do Something _g".
             Some special keyboard keys are supported as hotkeys, for example "#LEFT" would map to shift-left. 
             The keys supported like this are: LEFT, RIGHT, UP, DOWN, F1 .. F12, HOME, END, PGUP, PGDN.
             A hotkey text must be preceded with a space character ("MyMenu/Do_g" won't be interpreted as hotkey, while "MyMenu/Do _g" will).
            */
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
    }
}