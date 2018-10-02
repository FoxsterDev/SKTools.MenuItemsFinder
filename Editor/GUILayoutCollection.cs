using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SKTools.MenuItemsFinder
{
    internal partial class GUILayoutCollection
    {
        private static GUIStyle _toolbarSearchFieldStyle, _toolbarSearchFieldCancelButtonStyle;
       
        public static string SearchTextField(string text, params GUILayoutOption[] options)
        {
            if (_toolbarSearchFieldStyle == null || _toolbarSearchFieldCancelButtonStyle == null)
            {
                var editorStyles = (EditorStyles) typeof(EditorStyles)
                    .GetField("s_Current", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            
                _toolbarSearchFieldStyle = new GUIStyle((GUIStyle) typeof(EditorStyles)
                    .GetField("m_ToolbarSearchField", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(editorStyles))
                {
                    stretchWidth = true, 
                    stretchHeight = true
                };

                _toolbarSearchFieldCancelButtonStyle = new GUIStyle((GUIStyle) typeof(EditorStyles)
                    .GetField("m_ToolbarSearchFieldCancelButton", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetValue(editorStyles)); 
            }
            
            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            GUI.SetNextControlName("SearchTextField");
            text = GUILayout.TextField(text, _toolbarSearchFieldStyle, options);
            
            if (GUILayout.Button(string.Empty, _toolbarSearchFieldCancelButtonStyle))
            {
                text = string.Empty;
            }

            if (GUI.GetNameOfFocusedControl() != "SearchTextField")
            {
                GUI.FocusControl("SearchTextField");
            }
            
            GUILayout.EndHorizontal();
            return text;
        }
    }
}