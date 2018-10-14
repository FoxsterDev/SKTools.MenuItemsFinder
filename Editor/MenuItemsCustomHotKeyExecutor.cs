using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SKTools.MenuItemsFinder
{
    [InitializeOnLoad]
    internal class MenuItemsFinderKeyboardInput
    {
        private static FieldInfo eventInfo;
        
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

                string menuItemPath;
                MenuItemsFinder.HotKeysMap.TryGetValue(inputHotkey, out menuItemPath);
                
                if (!string.IsNullOrEmpty(menuItemPath))
                {
                    return EditorApplication.ExecuteMenuItem(menuItemPath);
                }
            }
            
            return false;
        }
    }
}
