using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SKTools.MenuItemsFinder
{
    internal partial class SKGUILayoutCollection
    {
        private static GUIStyle _toolbarSearchFieldStyle, _toolbarSearchFieldCancelButtonStyle;

        public static string SearchTextField(string text, bool focusControl = true, params GUILayoutOption[] options)
        {
            if (_toolbarSearchFieldStyle == null || _toolbarSearchFieldCancelButtonStyle == null)
            {
                var editorStyles = (EditorStyles) typeof(EditorStyles)
                    .GetField("s_Current", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);

                _toolbarSearchFieldStyle = new GUIStyle((GUIStyle) typeof(EditorStyles)
                    .GetField("m_ToolbarSearchField", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetValue(editorStyles))
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

            if (GUI.GetNameOfFocusedControl() != "SearchTextField" && focusControl)
            {
                GUI.FocusControl("SearchTextField");
            }

            GUILayout.EndHorizontal();
            return text;
        }
        
        public static  void SupportFooterBar(string version, string releaseNotesUrl, string readmeUrl, string askQuestionUrlInSkype)
        {
            GUILayout.Space (10);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("v" + version, EditorStyles.miniLabel))
            {
                Application.OpenURL(releaseNotesUrl);
            }

            GUILayout.FlexibleSpace ();
            if (GUILayout.Button("Readme", EditorStyles.miniLabel))
            {
                Application.OpenURL(readmeUrl);
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Ask a Question in Skype", EditorStyles.miniLabel))
            {
                Application.OpenURL(askQuestionUrlInSkype);
            }

            GUILayout.EndHorizontal();
        }
    }
}