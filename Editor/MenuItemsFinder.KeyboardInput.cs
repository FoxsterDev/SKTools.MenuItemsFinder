using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SKTools.MenuItemsFinder
{
    internal partial class MenuItemsFinder
    {
        private static FieldInfo _eventInfo;
        
        [InitializeOnLoadMethod]
        private static void MenuItemsFinder_KeyboardInput_Initializer()
        {
            _eventInfo = typeof(Event).GetField("s_Current", BindingFlags.Static | BindingFlags.NonPublic);
            
            EditorApplication.update += Update;
        }

        private static void Update()
        {
            var ev = (Event) _eventInfo.GetValue(null);
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
    }
}