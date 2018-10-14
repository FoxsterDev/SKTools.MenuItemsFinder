using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace SKTools.MenuItemsFinder
{
    [InitializeOnLoad]
    internal class MenuItemsFinderKeyboardInput
    {
        private static FieldInfo eventInfo;
        private static Dictionary<string, string> HotKeysMap;
        
        static MenuItemsFinderKeyboardInput()
        {
            eventInfo = typeof(Event).GetField("s_Current", BindingFlags.Static | BindingFlags.NonPublic);
            
            EditorApplication.update += Update;
        }

        private static void Update()
        {
            var ev = (Event) eventInfo.GetValue(null);
            var id = GUIUtility.GetControlID(FocusType.Passive);
            var eventType = ev.GetTypeForControl(id);
        
            if (eventType == EventType.KeyUp)
            {
                if (TryExecuteHotKey(ev))
                {
                    ev.Use();
                }
            }
        }

        private static bool TryExecuteHotKey(Event ev)
        {
            if (ev.isKey && (ev.shift || ev.alt || ev.command || ev.control))
            {
                var inputHotkey = new MenuItemHotKey
                {
                    Alt = ev.alt, 
                    Shift = ev.shift, 
                    Cmd = ev.command || ev.control,
                    Key = ev.keyCode.ToString().ToLower()
                };

                if (HotKeysMap == null)
                {
                    HotKeysMap = new Dictionary<string, string>();
                    foreach (var item in MenuItemsFinderPreferences.Current.CustomizedMenuItems)
                    {
                        foreach (var hotKey in item.CustomHotKeys)
                        {
                            HotKeysMap[hotKey] = item.Path;
                            Debug.Log("Added hotkey: "+ hotKey +" : "+ item.Path);
                        }
                    }
                }

                string menuItemPath;
                HotKeysMap.TryGetValue(inputHotkey, out menuItemPath);
                if (!string.IsNullOrEmpty(menuItemPath))
                {
                    return EditorApplication.ExecuteMenuItem(menuItemPath);
                }

                //Debug.Log("Can't find any menuitem with this hotkey="+ inputHotkey);
            }
            
            return false;
        }
    }
}
