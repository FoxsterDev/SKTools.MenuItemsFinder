using UnityEngine;

namespace SKTools.Module.Base
{
    internal interface GUIContainerInterface
    {
        GUIDelegate<Rect> DrawGuiCallback { get; set; }
        GUIDelegate<Rect> LostFocusCallback{ get; set; }
        GUIDelegate<Rect> CloseCallback{ get; set; }
        void Configurate();
        void Show();
    }
}