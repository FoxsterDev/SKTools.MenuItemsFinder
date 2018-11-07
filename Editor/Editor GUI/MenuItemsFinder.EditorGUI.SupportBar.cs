using SKTools.Base.Editor.GuiElementsSystem;
using SKTools.Module.RateMeWindow;
using UnityEngine;

namespace SKTools.MenuItemsFinder
{
    internal partial class MenuItemsFinder
    {
        private IGuiElement _supportBar;
       
        private void DrawSupportBar()
        {
            if (_supportBar == null)
            {
                _supportBar = GuiElementsFactory.CreateLayoutTemplateBarWithLeftAndRightAnchors();

                var leftAnchor = _supportBar.GetChild<GuiListElements>("LeftAnchor");
                leftAnchor.Add(GuiElementsFactory.CreateLayoutButtonWithMiniLabelStyle("v" + Version.Current, null));

                var rightAnchor = _supportBar.GetChild<GuiListElements>("RightAnchor");
                rightAnchor.Add(GuiElementsFactory.CreateLayoutButtonWithMiniLabelStyle("Read me",
                        () => { Application.OpenURL(@"https://github.com/FoxsterDev/SKTools.MenuItemsFinder/blob/master/README.md"); }))

                    .Add(GuiElementsFactory.CreateLayoutButtonWithMiniLabelStyle("Ask me in Skype",
                        () => { Application.OpenURL("skype:cachbroker?chat"); }))

                    .Add(GuiElementsFactory.CreateLayoutButtonWithMiniLabelStyle("Trello Board",
                        () => { Application.OpenURL("https://trello.com/b/7wgcSwgt/sktoolsmenuitemsfinder"); }))
                    
                    .Add(GuiElementsFactory.CreateLayoutButtonWithMiniLabelStyle("Rate Me",
                        () => {   
                            var rateMeAsset = _target.Assets.Get<TextAsset>("RateMeConfig");
                            var rateMeConfig = new RateMeConfig(rateMeAsset.text);
                            RateMe.Show(rateMeConfig);
                        }));
            }
            _supportBar.Draw();
        }
    }
}
