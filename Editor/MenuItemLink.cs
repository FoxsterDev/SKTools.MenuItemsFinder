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
        public List<MenuItemHotKey> HotKeys;// = new List<MenuItemHotKey>(1);

        [NonSerialized] private readonly MenuItemData _menuItem;
        [NonSerialized] public string Key;
        //[NonSerialized] public MenuItemHotKey HotKey;
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
            get { return Starred || !string.IsNullOrEmpty(CustomName) || IsMissed || HotKeys.Find(h => !h.IsOriginal && h.IsVerified) != null; }
        }

        public bool HasValidate
        {
            get { return _menuItem != null && _menuItem.TargetMethodValidate != null; }
        }

        public MenuItemLink(MenuItemData menuItem)
        {
            _menuItem = menuItem;
            HotKeys = new List<MenuItemHotKey>();
            Path = menuItem.TargetAttribute.menuItem;
            DeclaringType = menuItem.TargetMethod.DeclaringType;
            if (DeclaringType != null) AssemlyFilePath = DeclaringType.Assembly.Location;
            
            var hotKey = new MenuItemHotKey();
            var startIndex = -1;
            var hotkeyString = "";
            MenuItemHotKey.Extract(Path, out startIndex, out hotkeyString, out hotKey.Key, out hotKey.Shift, out hotKey.Alt, out hotKey.Cmd);
           
            if (startIndex > 0)
            {
                hotKey.IsOriginal = true;
                hotKey.IsVerified = true;
                HotKeys.Add(hotKey);
                Path = Path.Substring(0, startIndex - 1);
            }
            
            Key = Path.ToLower();
            
            UpdateLabel();
        }

        public void UpdateFrom(MenuItemLink item)
        {
            Starred = item.Starred;
            CustomNameEditable = CustomName = item.CustomName;
            HotKeys.AddRange(item.HotKeys);
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

            if (HotKeys.Count > 0 && !string.IsNullOrEmpty(HotKeys[0]))
            {
                Label = string.Concat(Label," <color=cyan>", HotKeys[0], "</color>");;
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