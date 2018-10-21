using UnityEditor;

namespace SKTools.Module.Base
{
    internal class Surrogate<T, K>  where T :  EditorWindow, GUIContainerInterface where K : AssetsProvider, new()
    {
        public readonly GUIContainerInterface Container;
        public readonly K Assets;
        
        public Surrogate(bool createIfNotExist, string assetsDirectory)
        {
            Assets = new K();
            
            Container = SKEditorWindow<T>.GetWindow(createIfNotExist);

            if (Container != null)
            {
                Assets.LoadAssets(assetsDirectory);
            }
        }
    }
}