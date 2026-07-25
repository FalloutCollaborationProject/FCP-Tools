using UnityEngine;
using Verse;

namespace FCP.Core.Robotics
{
    public class Dialog_RobotInfo : Window
    {
        private Vector2 scrollPosition;

        public override Vector2 InitialSize => new Vector2(620f, 640f);

        public Dialog_RobotInfo()
        {
            doCloseX = true;
            forcePause = true;
            absorbInputAroundWindow = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Color terminalColor = TerminalColors.PrimaryColor;

            Text.Font = GameFont.Medium;
            var titleRect = new Rect(0f, 0f, inRect.width, 35f);
            GUI.color = terminalColor;
            Widgets.Label(titleRect, "FCP_RobotInfo_Title".Translate());
            GUI.color = Color.white;

            Text.Font = GameFont.Small;
            var contentRect = new Rect(0f, 45f, inRect.width, inRect.height - 90f);
            DrawContent(contentRect, terminalColor);

            var closeRect = new Rect(inRect.width - 100f, inRect.height - 40f, 90f, 35f);
            if (Widgets.ButtonText(closeRect, "Close"))
            {
                Close();
            }
        }

        private void DrawContent(Rect rect, Color terminalColor)
        {
            Widgets.DrawMenuSection(rect);
            Rect innerRect = rect.ContractedBy(10f);

            TaggedString text = "FCP_RobotInfo_Body".Translate();
            float textHeight = Text.CalcHeight(text, innerRect.width - 20f);
            var viewRect = new Rect(0f, 0f, innerRect.width - 20f, textHeight + 10f);

            Widgets.BeginScrollView(innerRect, ref scrollPosition, viewRect);
            GUI.color = terminalColor;
            Widgets.Label(new Rect(0f, 0f, viewRect.width, textHeight), text);
            GUI.color = Color.white;
            Widgets.EndScrollView();
        }
    }
}
