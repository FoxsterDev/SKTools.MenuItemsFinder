using UnityEditor;

namespace SKTools.MenuItemsFinder
{
    internal partial class MenuItemsCollection
    {
        [MenuItem(MenuAssetPath + "Navigation/Open Player Settings", false, Priority)]
        public static void OpenPlayerSettings()
        {
            EditorApplication.ExecuteMenuItem("Edit/Project Settings/Player");
        }
    }
}
