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
        public string Notice;
        public List<MenuItemHotKey> CustomHotKeys;

        [NonSerialized] private readonly MenuItemData _menuItem;
        [NonSerialized] public string Key;
        [NonSerialized] public MenuItemHotKey HotKey;
        [NonSerialized] public string CustomNameEditable;
        [NonSerialized] public bool ShowNotice;
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
            get { return Starred || !string.IsNullOrEmpty(CustomName) || IsMissed || CustomHotKeys.Count > 0; }
        }

        public bool HasValidate
        {
            get { return _menuItem != null && _menuItem.TargetMethodValidate != null; }
        }

        public MenuItemLink(MenuItemData menuItem)
        {
            _menuItem = menuItem;
            CustomHotKeys = new List<MenuItemHotKey>(1);
            Path = menuItem.TargetAttribute.menuItem;
            DeclaringType = menuItem.TargetMethod.DeclaringType;
            if (DeclaringType != null) AssemlyFilePath = DeclaringType.Assembly.Location;

            ExtractHotKeyFromPath();
            
            Key = Path.ToLower();
            
            UpdateLabel();
        }

        private void ExtractHotKeyFromPath()
        {
            HotKey = new MenuItemHotKey();
            var startIndex = -1;
            var hotkeyString = "";
            MenuItemHotKey.Extract(Path, out startIndex, out hotkeyString, out HotKey.Key, out HotKey.Shift, out HotKey.Alt, out HotKey.Cmd);
           
            if (startIndex > -1)
            {
                HotKey.IsOriginal = true;
                HotKey.IsVerified = true;
                Path = Path.Substring(0, startIndex - 1);
            }
            else
            {
                HotKey = null;
            }
        }
        
        public void UpdateFrom(MenuItemLink item)
        {
            Starred = item.Starred;
            CustomNameEditable = CustomName = item.CustomName;
            CustomHotKeys = !IsMissed ? item.CustomHotKeys : new List<MenuItemHotKey>();
            
            if (string.IsNullOrEmpty(Path))
            {
                Path = item.Path;
                Key = Path.ToLower();
            }

            UpdateLabel();
        }

        public void UpdateLabel()
        {
            Label = !string.IsNullOrEmpty(CustomName) ? CustomName : Path;

            var hotkey = HotKey ?? (CustomHotKeys.Count > 0 ? CustomHotKeys[0] : null);
            if (hotkey != null)
            {
                Label = string.Concat(Label," <color=cyan>", hotkey.Formatted, "</color>");;
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
    }
}