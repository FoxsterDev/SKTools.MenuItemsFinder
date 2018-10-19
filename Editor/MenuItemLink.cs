using System;


namespace SKTools.MenuItemsFinder
{
    [Serializable]
    internal partial class MenuItemLink
    {
        partial void UpdateHotKeys(MenuItemLink item = null);
        partial void HasCustomHotKeys(ref bool has);
      
        public string CustomName;
        public bool Starred;
        public string Path;
        public string Notice;
        
        [NonSerialized] private readonly MenuItemData _menuItem;
        [NonSerialized] public string Key;
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
            get
            {
                var hasCustomHotKeys = false;
                HasCustomHotKeys(ref hasCustomHotKeys);
                return Starred || !string.IsNullOrEmpty(CustomName) || IsMissed || hasCustomHotKeys;
            }
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
            if (DeclaringType != null) AssemlyFilePath = DeclaringType.Assembly.Location;
 
            UpdateLabel();
            UpdateHotKeys();
            
            Key = Path.ToLower();
        }
        
        public void UpdateFrom(MenuItemLink item)
        {
            Starred = item.Starred;
            CustomNameEditable = CustomName = item.CustomName;
            
            if (string.IsNullOrEmpty(Path))
            {
                Path = item.Path;
            }
            
            UpdateLabel();
            UpdateHotKeys(item);
            
            Key = Path.ToLower();
        }
       
        public void UpdateLabel()
        {
            Label = !string.IsNullOrEmpty(CustomName) ? CustomName : Path;
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
    }
}