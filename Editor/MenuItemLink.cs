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
        public readonly string SearchingKey;

        public bool Starred;

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
            //% (ctrl on Windows, cmd on macOS), # (shift), & (alt).
            Label = menuItem.TargetAttribute.menuItem;// .menuItem;
            var index = Label.LastIndexOf(' ');
            if (index > -1)
            {
                var s = Label.Substring(index);
                var k = s;
                if (s.Contains('%') || s.Contains('#') || s.Contains('&'))
                {
                        s =
    #if UNITY_EDITOR_OSX
                    s.Replace("%", "cmd+").
    #else
                    s.Replace("%", "ctrl+").
    #endif
                    Replace("#", "shift+").Replace("&", "alt+");
                    s = string.Concat("<color=cyan>", s, "</color>");
                    Label = Label.Replace(k, s);
                }
            }

            SearchingKey = Label.ToLower();
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
            if (CanExecute())
            {
                _menuItem.TargetMethod.Invoke(null, null);
            }
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