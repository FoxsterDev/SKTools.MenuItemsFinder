using UnityEngine;

namespace SKTools.MenuItemsFinder
{
    internal partial class MenuItemsFinder
    {
        private void DrawUnavailableState(Rect position)
        {
            //pivot = new Vector2(position.xMin + position.width * 0.5f, position.yMin + position.height * 0.5f);
            //var matrixBackup = GUI.matrix;
            //GUIUtility.RotateAroundPivot(angle%360, pivot);
            var width = _target.Assets.LoadingImage.width;
            var height = _target.Assets.LoadingImage.height;
            var rect = new Rect(
                position.width * 0.5f - width * 0.5f, position.height * 0.5f - height * 0.5f,
                width,
                height);
            GUI.DrawTexture(rect, _target.Assets.LoadingImage);
            //GUI.matrix = matrixBackup;
        }
    }
}
