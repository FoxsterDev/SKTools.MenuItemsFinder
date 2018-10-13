using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SKTools.MenuItemsFinder
{
    [InitializeOnLoad]
    internal class MenuItemsFinderKeyboardInput
    {
        private static FieldInfo eventInfo;
        
        static  MenuItemsFinderKeyboardInput()
        {
            eventInfo = typeof(Event).GetField("s_Current", BindingFlags.Static | BindingFlags.NonPublic);
            EditorApplication.update += Update;
        }

        private static void Update()
        {
            var ev = (Event)eventInfo.GetValue(null);
            int id = GUIUtility.GetControlID(FocusType.Keyboard);
 
            if (ev.GetTypeForControl(id) == EventType.KeyDown)
            {
                //Debug.Log("1 pressed!");
                //Debug.Log(ev.type +" : "+ ev.character +" : "+ ev.shift);
                /*if (ev.keyCode == KeyCode.Keypad1)
                {
                    Debug.Log("1 pressed!");
                    // Causes repaint & accepts event has been handled
                    GUIUtility.hotControl = id;
                    Event.current.Use();
                }*/
            }
        }
    }
}