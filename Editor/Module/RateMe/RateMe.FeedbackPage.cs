using System.Text;
using SKTools.Base.Editor;
using UnityEngine;

namespace SKTools.RateMeWindow
{
    internal partial class RateMe
    {
        private string _feedbackMessage;
        
        private void DrawFeedbackGui(IGUIContainer window)
        {
            var position = window.Position;

            GUI.Label(new Rect(5, 5, position.width, 24), Config.FeedbackTitle);

            _feedbackMessage = GUI.TextArea(new Rect(5, 24, position.width - 10, position.height - 128),
                _feedbackMessage);

            if (GUI.Button(new Rect(position.width / 2 - 156, position.height - 64, 156, 64), "Back",
                _targetGui.Assets.ButtonStyle))
            {
                window.DrawGuiCallback = DrawRateGui;
                window.Repaint();
                return;
            }

            if (GUI.Button(new Rect(position.width / 2 + 5, position.height - 64, 156, 64), "Send Email",
                _targetGui.Assets.ButtonStyle))
                Email();

            window.Repaint();
        }

        private void Email()
        {
            var builder = new StringBuilder();
            builder.Append("mailto:" + Config.EmailAddress);
            builder.Append("?subject=" + EscapeURL(Config.EmailSubject));
            builder.Append("&body=" + EscapeURL(_feedbackMessage + Config.EmailAdditionalInfoFormat));

            Application.OpenURL(builder.ToString());
        }

        private string EscapeURL(string url)
        {
            return WWW.EscapeURL(url).Replace("+", "%20");
        }
    }
}