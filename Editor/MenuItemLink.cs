using System;
using System.Collections.Generic;
using UnityEditor;
using Object = UnityEngine.Object;

namespace SKTools.MenuItemsFinder
{
    [Serializable]
    internal partial class MenuItemLink
    {
        /// <summary>
        /// Shared data between items for getting context, this is faster than GetComponent..
        /// </summary>
        private static readonly Dictionary<string, object[]> MenuCommandSharedContext = new Dictionary<string, object[]>(5);
        /// <summary>
        /// Clear cache
        /// </summary>
        private static int _activeInstanceId;


        [NonSerialized] private readonly MenuItemData _menuItem;

        public string CustomName;
        public bool Starred;
        public string Path;
        public string Notice;
        
        [NonSerialized] public string Key;
        [NonSerialized] public string CustomNameEditable;
        [NonSerialized] public string AssemblyName;
        [NonSerialized] public bool ShowNotice;
        [NonSerialized] public bool IsRemoved;
        [NonSerialized] public bool IsFiltered;
        [NonSerialized] public bool IsContextMenu;
        [NonSerialized] public bool IsUnityMenu;

        public string Label { get; private set; }
        public string AssemlyFilePath { get; private set; }
        public Type DeclaringType { get; private set; }
        private bool HasMenuCommandParameter = false;
        private string MenuCommandContextType;
        
        public bool IsMissed
        {
            get { return _menuItem == null; }
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
            if (DeclaringType != null)
            {
                AssemlyFilePath = DeclaringType.Assembly.Location;
                AssemblyName = DeclaringType.Assembly.GetName().Name;
                IsUnityMenu = AssemblyName.Split('.')[0].Contains("Unity");
            }
            
            IsContextMenu = Path.Substring(0, 7) == "CONTEXT";

            if (menuItem.TargetMethod.GetParameters().Length > 0)
            {
                var parameter = menuItem.TargetMethod.GetParameters()[0];
                
                if (parameter.ParameterType == typeof(MenuCommand))
                {
                    HasMenuCommandParameter = true;
                    var startIndex = IsContextMenu ? 8 : 0;
                    var endIndex = Path.IndexOf('/', startIndex + 1);
                    MenuCommandContextType = Path.Substring(startIndex, endIndex - startIndex);
                }
            }
            Key = Path.ToLower();
            
            UpdateLabel();
        }
        
        public void UpdateFrom(MenuItemLink item)
        {
            Starred = item.Starred;
            CustomNameEditable = CustomName = item.CustomName;
            
            if (string.IsNullOrEmpty(Path))
            {
                Path = item.Path;
            }
            
            IsContextMenu = Path.Substring(0, 7) == "CONTEXT";
            Key = Path.ToLower();

            UpdateLabel();
        }
       
        public void UpdateLabel()
        {
            Label = !string.IsNullOrEmpty(CustomName) ? CustomName : Path;
            if (IsMissed)
            {
                Label = string.Concat("<color=red>", "[Missed]", "</color>", Label);
            }
        }

        public bool CanExecute()
        { 
            string error;
            var parameters = GetParameters(out error);
            if (!string.IsNullOrEmpty(error))
                return false;
            
            if (HasValidate)
            {
                return (bool) _menuItem.TargetMethodValidate.Invoke(null, parameters);
            }

            return true;
        }
      
        public void Execute()
        {
            string error;
            var parameters = GetParameters(out error);
            if (string.IsNullOrEmpty(error))
            {
                _menuItem.TargetMethod.Invoke(null, parameters);
            }
            else
            {
                throw new Exception(error);
            }
        }
        
        private object[] GetParameters(out string error)
        {
            error = null;
            
            if (HasMenuCommandParameter)
            {
                if (Selection.activeObject == null)
                    return null;
                
                if (_activeInstanceId != Selection.activeInstanceID)
                {
                    MenuCommandSharedContext.Clear();
                    _activeInstanceId = Selection.activeInstanceID;
                }
                Object context = null;
                
                object[] parameters;
                if (MenuCommandSharedContext.ContainsKey(MenuCommandContextType))
                {
                    parameters = MenuCommandSharedContext[MenuCommandContextType];
                    if (parameters == null)
                    {
                        error = "Cant get menucommand parameter of type " + MenuCommandContextType;
                        return null;
                    }

                    return parameters;
                }

                var activeObjectType = Selection.activeObject.GetType().Name;
                if (activeObjectType.Equals(MenuCommandContextType))
                {
                    context = Selection.activeObject;
                }
                else if (Selection.activeGameObject != null)
                {
                    context = Selection.activeGameObject.GetComponent(MenuCommandContextType);
                }

                if (context != null)
                {
                    parameters = new object[] {new MenuCommand(context)};
                    MenuCommandSharedContext[MenuCommandContextType] = parameters;
                    return parameters;
                }

                error = "Cant get menucommand parameter of type " + MenuCommandContextType;
                MenuCommandSharedContext[MenuCommandContextType] = null;
            }

            return null;
        }

        public override string ToString()
        {
            return string.Format("MenuItem={0}; Method={1}", Label, !IsMissed ? _menuItem.TargetMethod.ToString() : "[Missed]");
        }
    }
}