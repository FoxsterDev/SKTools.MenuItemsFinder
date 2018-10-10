using System;
using System.Collections.Generic;


namespace SKTools.MenuItemsFinder
{
    [Serializable]
    internal class MenuItemLink
    {
        public string CustomName;
        public bool Starred;
        public string Path;
      
        [NonSerialized] private readonly MenuItemData _menuItem;
        [NonSerialized] public string Key;
        [NonSerialized] public string HotKey;
        [NonSerialized] public string HotKeyFormatted;
        [NonSerialized] public string CustomNameEditable;
        [NonSerialized] public bool IsRemoved;
        [NonSerialized] public bool IsFiltered;

        public string Label { get; private set; }
        public string AssemlyFilePath { get; private set; }
        public Type DeclaringType { get; private set; }

        public bool IsMissed
        {
            get { return _menuItem == null; }
        }

        public bool IsCustomized
        {
            get { return Starred || !string.IsNullOrEmpty(CustomName) || IsMissed; }
        }

        public bool HasValidate
        {
            get { return _menuItem != null && _menuItem.TargetMethodValidate != null; }
        }

        public MenuItemLink(MenuItemData menuItem)
        {
            _menuItem = menuItem;
            Path = menuItem.TargetAttribute.menuItem;
            DeclaringType = menuItem.TargetMethod.DeclaringType;
            AssemlyFilePath = DeclaringType.Assembly.Location;

            Update();
        }

        public void UpdateFrom(MenuItemLink item)
        {
            Starred = item.Starred;
            CustomNameEditable = CustomName = item.CustomName;
            if (string.IsNullOrEmpty(Path))
            {
                Path = item.Path;
            }

            Update();
        }

        public void Update()
        {
            var hotkeyStartIndex = -1;
            FindHotKey(Path, out hotkeyStartIndex, out HotKey);

            if (hotkeyStartIndex > -1)
            {
                HotKeyFormatted = ToFormatHotKey(HotKey);
                Path = Path.Substring(0, hotkeyStartIndex - 1);
            }

            Key = Path.ToLower();
            
            UpdateLabel();
        }

        public void UpdateLabel()
        {
            if (!string.IsNullOrEmpty(CustomName))
            {
                Label = string.Concat(CustomName, " ", HotKeyFormatted);
            }
            else
            {
                Label = string.Concat(Path, " ", HotKeyFormatted);
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
            return string.Format("MenuItem={0}; Method={1}", Label, !IsMissed ? _menuItem.TargetMethod.ToString() : "[Missed]");
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


        /*
        * To create a hotkey you can use the following special characters: % (ctrl on Windows, cmd on macOS), # (shift), & (alt). If no special modifier key combinations are required the key can be given after an underscore. For example to create a menu with hotkey shift-alt-g use "MyMenu/Do Something #&g".
         * To create a menu with hotkey g and no key modifiers pressed use "MyMenu/Do Something _g".
         Some special keyboard keys are supported as hotkeys, for example "#LEFT" would map to shift-left. 
         The keys supported like this are: LEFT, RIGHT, UP, DOWN, F1 .. F12, HOME, END, PGUP, PGDN.
         A hotkey text must be preceded with a space character ("MyMenu/Do_g" won't be interpreted as hotkey, while "MyMenu/Do _g" will).
        */
        internal static void FindHotKey(string itemPath, out int index, out string hotkey)
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
    }
}