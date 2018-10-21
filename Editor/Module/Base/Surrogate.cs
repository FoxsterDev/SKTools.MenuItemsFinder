using UnityEditor;

namespace SKTools.Module.Base
{
    internal class Surrogate<T, K>  where T :  EditorWindow, IGUIContainer where K : AssetsProvider, new()
    {
        public readonly IGUIContainer Container;
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