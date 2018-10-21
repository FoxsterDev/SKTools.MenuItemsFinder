using UnityEditor;
using UnityEngine;

namespace SKTools.Module.Base
{
    public delegate void GUIDelegate<T>(T obj);


    internal abstract class SKEditorWindow<T> : EditorWindow where T : EditorWindow, GUIContainerInterface
    {
        public GUIDelegate<Rect> DrawGuiCallback { get; set; }
        public GUIDelegate<Rect> LostFocusCallback { get; set; }
        public GUIDelegate<Rect> CloseCallback { get; set; }
        
        private static bool IsCreated
        {
            get { return EditorPrefs.GetBool(typeof(T).Name, false); }
            set { EditorPrefs.SetBool(typeof(T).Name, value); }
        }

        /// <summary>
        /// I added this method because the standart GetWindow immediately create and show window, and I cant configure it before opening
        /// </summary>
        /// <param name="createIfNotExist">In some cases I need to check of exisiting already opened window</param>
        /// <typeparam name="T">Some type of editor window</typeparam>
        /// <returns>Return a window of type T</returns>
        public static T GetWindow(bool createIfNotExist = false)
        {
            if (!createIfNotExist && !IsCreated) return null;

            T window;
            var objectsOfTypeAll = Resources.FindObjectsOfTypeAll(typeof(T));
            if (objectsOfTypeAll.Length < 1)
            {
                if (!createIfNotExist)
                {
                    IsCreated = false;
                    return null;
                }

                window = ScriptableObject.CreateInstance<T>();
            }
            else
            {
                window = (T) objectsOfTypeAll[0];
            }

            window.Configurate();
            return window;
        }

        public virtual void Configurate()
        {
            titleContent = GetTitleContent;
            if (GetMinSize.HasValue)
            {
                minSize = GetMinSize.Value;
            }

            if (GetMaxSize.HasValue)
            {
                maxSize = GetMaxSize.Value;
            }

            autoRepaintOnSceneChange = GetAutoRepaintOnSceneChange;
        }

        protected virtual Vector2? GetMinSize
        {
            get { return null; }
        }

        protected virtual Vector2? GetMaxSize
        {
            get { return null; }
        }

        protected virtual GUIContent GetTitleContent
        {
            get { return null; }
        }

        protected virtual bool GetAutoRepaintOnSceneChange
        {
            get { return false; }
        }
        
        protected virtual bool GetAutoRepaintOnSelectionChange
        {
            get { return false; }
        }

        private void Awake()
        {
            IsCreated = true;
        }

        /// <summary>
        /// we can easily switch gui content of this window
        /// </summary>
        private void OnGUI()
        {
            if (DrawGuiCallback != null) DrawGuiCallback(position);
        }

        /// <summary>
        /// Some menuitems requires a validating  method, and it need to update visual state of items
        /// </summary>
        private void OnSelectionChange()
        {
            if (GetAutoRepaintOnSelectionChange)
            {
                Repaint();
            }
        }

        /// <summary>
        /// This callbacks are used for saving state
        /// </summary>
        private void OnLostFocus()
        {
            if (LostFocusCallback != null) LostFocusCallback(position);
        }

        private void OnDestroy()
        {
            IsCreated = false;

            if (CloseCallback != null) CloseCallback(position);

            DrawGuiCallback = null;
            LostFocusCallback = null;
            CloseCallback = null;
        }
    }
}