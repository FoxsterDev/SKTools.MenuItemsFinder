using SKTools.Base.Editor;
using UnityEditor;
using UnityEngine;

namespace SKTools.RateMeWindow
{
    internal partial class RateMe
    {
        private int _starsCount;
        private Rect[] _starRects;
       
        private float SizeStar
        {
            get { return (_targetGui.Assets.StarredImage.width + _targetGui.Assets.StarredImage.height) * 0.5f; }
        }
        
        private void DrawRateGui(IGUIContainer window)
        {
            var position = window.Position;

            var height = (position.height - SizeStar) * 0.5f;
            var rectUpper = new Rect(0, 0, position.width, height);

            GUI.Label(rectUpper, Config.RequestLabel, _targetGui.Assets.LabelStyle);

            var offsetX = (position.width / Config.MaxStar - SizeStar) * 0.5f;
            var rect = new Rect(0, height, SizeStar, SizeStar);

            for (var i = 0; i < Config.MaxStar; i++)
            {
                rect.x += offsetX;
                _starRects[i] = rect;
                if (GUI.Button(_starRects[i], string.Empty, _targetGui.Assets.StarStyles[i]))
                {
                    _starsCount = i + 1;
                    SetCountStar(_starsCount);
                    return;
                }

                rect.x += SizeStar + offsetX;
            }

            if (GUI.Button(new Rect((position.width - 128) / 2, position.height - 64, 128, 64), Config.RateButtonText,
                _targetGui.Assets.ButtonStyle))
            {
                if (_starsCount <= Config.MinStar)
                {
                    window.DrawGuiCallback = DrawFeedbackGui;
                    window.Repaint();
                    return;
                }

                var ok = EditorUtility.DisplayDialog("Please take a look", Config.GithubMessage,
                    Config.RateGithubButtonText, Config.RateAssetStoreButtonText);
                if (ok)
                    Application.OpenURL(Config.GithubUrl);
                else
                    Application.OpenURL(Config.AssetStoreUrl);
            }

            if (Event.current.type == EventType.Repaint)
            {
                var countStar = _starsCount;

                for (var i = 0; i < Config.MaxStar; i++)
                    if (_starRects[i].Contains(Event.current.mousePosition))
                        countStar = i + 1;

                SetCountStar(countStar);
            }

            window.Repaint();
        }

        private void SetCountStar(int countStar)
        {
            for (var i = 0; i < Config.MaxStar; i++)
                _targetGui.Assets.StarStyles[i] = i < countStar
                    ? _targetGui.Assets.StarredButtonStyle
                    : _targetGui.Assets.UnstarredButtonStyle;
        }
    }
}