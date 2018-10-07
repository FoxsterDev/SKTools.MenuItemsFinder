using System;
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
        
        private static readonly char[] hotKeysIndicators = new []
        {
            '%', '#', '_', '&'
        };
        
        public bool Starred;

        public string CustomName;
        
        public string MenuItemPath
        {
            get { return _menuItem.TargetAttribute.menuItem; }
        }

        public readonly string HotKey;
        
        public bool HasValidate
        {
            get { return _menuItem.TargetMethodValidate != null; }
        }
        
        public MenuItemLink(MenuItemData menuItem)
        {
            _menuItem = menuItem;
            //% (ctrl on Windows, cmd on macOS), # (shift), & (alt).
            Label = menuItem.TargetAttribute.menuItem;
            var hotkeyStartIndex = -1;
            FindHotKey(Label, out hotkeyStartIndex, out HotKey);
            Label = Label.Substring(0, hotkeyStartIndex);
            Key = Label.ToLower();//without hotkey
        }

        /*
         * To create a hotkey you can use the following special characters: % (ctrl on Windows, cmd on macOS), # (shift), & (alt). If no special modifier key combinations are required the key can be given after an underscore. For example to create a menu with hotkey shift-alt-g use "MyMenu/Do Something #&g". To create a menu with hotkey g and no key modifiers pressed use "MyMenu/Do Something _g".
          Some special keyboard keys are supported as hotkeys, for example "#LEFT" would map to shift-left. The keys supported like this are: LEFT, RIGHT, UP, DOWN, F1 .. F12, HOME, END, PGUP, PGDN.
          A hotkey text must be preceded with a space character ("MyMenu/Do_g" won't be interpreted as hotkey, while "MyMenu/Do _g" will).
         */
        
        internal static void FindHotKey(string itemPath, out int index, out string hotkey)
        {
            hotkey = string.Empty;

            //var chars = itemPath.ToCharArray();
            if (itemPath[itemPath.Length - 1] != ' ')
            {
                var splitted = itemPath.Split(hotKeysIndicators);
                //chars. .Contains(hotKeysIndicators)
            }
            
            index = itemPath.LastIndexOf(' ');
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
            }
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
    }
}