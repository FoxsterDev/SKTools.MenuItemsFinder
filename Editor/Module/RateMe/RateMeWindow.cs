using SKTools.Base.Editor;
using UnityEngine;

namespace SKTools.RateMeWindow
{
    /// <summary>
    /// This window will show rate me window
    /// </summary>
    internal class Window : CustomEditorWindow<Window>, IGUIContainer
    {
        protected override GUIContent GetTitleContent
        {
            get { return new GUIContent("Rate Me"); }
        }

        protected override Vector2? GetMinSize
        {
            get { return new Vector2(400, 400); }
        }

        protected override Vector2? GetMaxSize
        {
            get { return new Vector2(400, 400); }
        }

        public Rect Position
        {
            get { return position; }
        }
    }
}