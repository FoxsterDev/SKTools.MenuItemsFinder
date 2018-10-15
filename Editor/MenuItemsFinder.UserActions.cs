using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace SKTools.MenuItemsFinder
{
    internal partial class MenuItemsFinder
    {
        private MenuItemLink _selectedMenuItem;

        private void ClickButton_SetCustomName(MenuItemLink link)
        {
            if (_selectedMenuItem != null && !string.IsNullOrEmpty(_selectedMenuItem.CustomNameEditable))
            {
                link.CustomName = _selectedMenuItem.CustomNameEditable;
                link.UpdateLabel();
            }
        }

        private void ClickButton_Remove(MenuItemLink item)
        {
            _wasRemoving = true;
            item.IsRemoved = true;
        }
        
        private bool ClickButton_ToggleSettings(MenuItemLink item)
        {
            if (_selectedMenuItem == null)
            {
                SelectItem(item);
                return true;
            }

            var newSettings = _selectedMenuItem.Key != item.Key;
            UnSelectItem();

            if (newSettings)
            {
                SelectItem(item);
                return true;
            }

            return false;
        }

        private void ClickButton_ToggleStarred(MenuItemLink item)
        {
            item.Starred = !item.Starred;
        }

        private void ClickButton_TryOpenFileThatContainsThisMenuItem(MenuItemLink item, out string error)
        {
            var fullPath = FindScriptWhichContainsMenuItem(item, out error);
            if (!string.IsNullOrEmpty(fullPath))
            {
                OpenFile(fullPath);
                EditorGUIUtility.systemCopyBuffer = item.Path;
            }
        }
    }
}