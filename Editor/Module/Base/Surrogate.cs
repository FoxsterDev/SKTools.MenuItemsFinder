using UnityEditor;

namespace SKTools.Module.Base
{
    internal class Surrogate<T, K>  where T : EditorWindow, GUIContainerInterface where K : AssetsProvider, new()
    {
        public readonly GUIContainerInterface Container;
        public readonly K Assets;
        
        public Surrogate(bool createWindowIfNotExist = false)
        {
            Assets = new K();
            Container = SKEditorWindow<T>.GetWindow(createWindowIfNotExist);
        }
    }
}