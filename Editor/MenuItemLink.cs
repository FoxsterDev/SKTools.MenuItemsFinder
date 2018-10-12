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
        public string Description;
        public List<MenuItemHotKey> CustomHotKeys;

        [NonSerialized] private readonly MenuItemData _menuItem;
        [NonSerialized] public string Key;
        [NonSerialized] public MenuItemHotKey HotKey;
        [NonSerialized] public string CustomNameEditable;
        [NonSerialized] public bool ShowDescription;
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
            CustomHotKeys = new List<MenuItemHotKey>();
            Path = menuItem.TargetAttribute.menuItem;
            DeclaringType = menuItem.TargetMethod.DeclaringType;
            if (DeclaringType != null) AssemlyFilePath = DeclaringType.Assembly.Location;
            PostUpdate();
        }

        public void UpdateFrom(MenuItemLink item)
        {
            Starred = item.Starred;
            CustomNameEditable = CustomName = item.CustomName;
            if (string.IsNullOrEmpty(Path))
            {
                Path = item.Path;
            }

            PostUpdate();
        }

        public void PostUpdate()
        {
            HotKey = new MenuItemHotKey(Path);
            if (HotKey.StartIndex > 0)
            {
                Path = Path.Substring(0, HotKey.StartIndex - 1);
            }

            Key = Path.ToLower();
            
            UpdateLabel();
        }

        public void UpdateLabel()
        {
            Label = !string.IsNullOrEmpty(CustomName) ? CustomName : Path;

            if (!string.IsNullOrEmpty(HotKey))
            {
                Label += " " + HotKey;
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