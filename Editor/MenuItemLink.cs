﻿using System;
using System.Collections.Generic;
using System.Linq;
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
        
        //[NonSerialized]
        public string OriginalPath
        {
            get { return _menuItem.TargetAttribute.menuItem; }
        }

        public string OriginalName;
      
        //public string Notice;
        
        //[NonSerialized] 
        public string Key;
        [NonSerialized] public string EditName;
        [NonSerialized] public string AssemblyName;
        //[NonSerialized] public bool ShowNotice;
        [NonSerialized] public bool IsRemoved;
        [NonSerialized] public bool IsFiltered;
        [NonSerialized] public bool IsContextMenu;
        [NonSerialized] public bool IsUnityMenu;
       // [NonSerialized] public bool IsEditable;
        [NonSerialized] public bool IsEditName;
        [NonSerialized] public bool IsEditHotkey;

        public string Label { get; private set; }
        public string AssemlyFilePath { get; private set; }
        public Type DeclaringType { get; private set; }
        private readonly bool _hasMenuCommandParameter = false;
        private readonly string _menuCommandContextType;
        
        public bool IsMissed
        {
            get { return _menuItem == null; }
        }

        private bool HasValidate
        {
            get { return _menuItem != null && _menuItem.TargetMethodValidate != null; }
        }

        public MenuItemLink(MenuItemData menuItem)
        {
            _menuItem = menuItem;

            OriginalName = CustomName = OriginalPath;
            Key = OriginalPath.ToLower();
            
            DeclaringType = menuItem.TargetMethod.DeclaringType;
            if (DeclaringType != null)
            {
                AssemlyFilePath = DeclaringType.Assembly.Location;
                AssemblyName = DeclaringType.Assembly.GetName().Name;
                IsUnityMenu = AssemblyName.Split('.')[0].Contains("Unity");
            }
            
            IsContextMenu = Key.Substring(0, 7) == "context";

            if (menuItem.TargetMethod.GetParameters().Length > 0)
            {
                var parameter = menuItem.TargetMethod.GetParameters()[0];
                
                if (parameter.ParameterType == typeof(MenuCommand))
                {
                    _hasMenuCommandParameter = true;
                    var startIndex = IsContextMenu ? 8 : 0;
                    var endIndex = OriginalPath.IndexOf('/', startIndex + 1);
                    _menuCommandContextType = OriginalPath.Substring(startIndex, endIndex - startIndex);
                }
            }
        }
        
        public void UpdateFrom(MenuItemLink item)
        {
            Starred = item.Starred;
            CustomName = item.CustomName;
            
            if (string.IsNullOrEmpty(Key))
            {
                Key = item.Key;
            }
        }
       
        public void UpdateLabel()
        {
            Label = !string.IsNullOrEmpty(CustomName) ? CustomName : OriginalName;
             
            if (!string.IsNullOrEmpty(AssemblyName))
            {
                Label = string.Concat(Label, "  (Assembly: " + AssemblyName + ")");
            }

            if (IsMissed)
            {
                Label = string.Concat("<color=red>", "[Missed]", "</color>", Label);
            }
        }

        public bool CanExecute()
        {
            if (IsMissed)
                return false;
            
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
            
            if (_hasMenuCommandParameter)
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
                if (MenuCommandSharedContext.ContainsKey(_menuCommandContextType))
                {
                    parameters = MenuCommandSharedContext[_menuCommandContextType];
                    if (parameters == null)
                    {
                        error = "Cant get menucommand parameter of type " + _menuCommandContextType;
                        return null;
                    }

                    return parameters;
                }

                var activeObjectType = Selection.activeObject.GetType().Name;
                if (activeObjectType.Equals(_menuCommandContextType))
                {
                    context = Selection.activeObject;
                }
                else if (Selection.activeGameObject != null)
                {
                    context = Selection.activeGameObject.GetComponent(_menuCommandContextType);
                }

                if (context != null)
                {
                    parameters = new object[] {new MenuCommand(context)};
                    MenuCommandSharedContext[_menuCommandContextType] = parameters;
                    return parameters;
                }

                error = "Cant get menucommand parameter of type " + _menuCommandContextType;
                MenuCommandSharedContext[_menuCommandContextType] = null;
            }

            return null;
        }

        public override string ToString()
        {
            return string.Format("MenuItem={0}; Method={1}", Label, !IsMissed ? _menuItem.TargetMethod.ToString() : "[Missed]");
        }
    }
}