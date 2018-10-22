using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace SKTools.MenuItemsFinder
{
    internal partial class MenuItemsFinder
    {
        /// <summary>
        /// Default event.current has this signature:  if (GUIUtility.guiDepth > 0) return Event.s_Current; else return (Event) null;
        // and we cant get existing event without gui handler
        /// </summary>
        private static FieldInfo _eventInfo;

        /// <summary>
        /// Static because to prevent several hotKeysMap.
        /// In unity editor are possible to drop instance of variables state.
        /// For example not only clearly recompiling, but and just open some instance editorwindow
        /// We could be create an instance ScriptableObject,  but it helps with only serializable type, not dictionary
        /// </summary>
        private static Dictionary<string, string> _hotKeysMap;

        [InitializeOnLoadMethod]
        private static void MenuItemsFinder_HotKeysExecutor_Initializer()
        {
            DiagnosticRun(() =>
            {
                _eventInfo = typeof(Event).GetField("s_Current", BindingFlags.Static | BindingFlags.NonPublic);
                UpdateHotKeysMap(GetFinder()._prefs.CustomizedMenuItems);
                EditorApplication.update += KeyboardInputUpdate;
            });
        }

        private static void UpdateHotKeysMap(List<MenuItemLink> menuItemLinks)
        {
            _hotKeysMap = new Dictionary<string, string>(menuItemLinks.Count);

            foreach (var item in menuItemLinks)
            {
                foreach (var hotKey in item.CustomHotKeys)
                {
                    if (!hotKey.IsVerified) continue;
                    _hotKeysMap[hotKey] = item.Path;
                }
            }
        }

        /// <summary>
        ///  ev.keyCode only filled when happens KeyUp, another cases with character obuse to match keyboard simbols , if the char is not A-z
        /// </summary>
        private static void KeyboardInputUpdate()
        {
            var ev = (Event) _eventInfo.GetValue(null);
            if (ev == null) return;
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

        /// <summary>
        /// Now it uses EditorApplication.ExecuteMenuItem , but may be there needs to load a finder and check validation
        /// </summary>
        /// <param name="ev"></param>
        /// <returns></returns>
        private static bool TryExecuteHotKey(Event ev)
        {
            var inputHotkey = ConvertEventToHotKey(ev);

            if (inputHotkey != null)
            {
                string menuItemPath;
                _hotKeysMap.TryGetValue(inputHotkey, out menuItemPath);

                if (!string.IsNullOrEmpty(menuItemPath))
                {
                    return EditorApplication.ExecuteMenuItem(menuItemPath);
                }
            }

            return false;
        }

        private static MenuItemHotKey ConvertEventToHotKey(Event ev)
        {
            if (ev.isKey && (ev.shift || ev.alt || ev.command || ev.control))
            {
                return new MenuItemHotKey
                {
                    Alt = ev.alt,
                    Shift = ev.shift,
                    Cmd = ev.command || ev.control,
                    Key = ev.keyCode.ToString().ToLower()
                };
            }

            return null;
        }
    }
}