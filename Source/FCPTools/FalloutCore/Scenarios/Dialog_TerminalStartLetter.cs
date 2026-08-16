using UnityEngine;
using Verse.Sound;

namespace FCP.Core;

public class Dialog_TerminalStartLetter : Window
{
    private readonly string dialogTitle;
    private readonly string bodyText;
    private readonly SoundDef closeSound;
    private Vector2 scrollPosition;

    public override Vector2 InitialSize => new Vector2(720f, 560f);

    public Dialog_TerminalStartLetter(string title, string text, SoundDef closeSound)
    {
        dialogTitle = title;
        bodyText = text;
        this.closeSound = closeSound;
        doCloseX = false;
        forcePause = true;
        absorbInputAroundWindow = true;
        closeOnClickedOutside = false;
        closeOnAccept = false;
        closeOnCancel = false;
    }

    public override void DoWindowContents(Rect inRect)
    {
        Color terminalColor = TerminalColors.PrimaryColor;

        Text.Font = GameFont.Medium;
        GUI.color = terminalColor;
        Widgets.Label(new Rect(0f, 0f, inRect.width, 35f), dialogTitle);
        Text.Font = GameFont.Small;
        Widgets.DrawLineHorizontal(0f, 38f, inRect.width);
        GUI.color = Color.white;

        Rect contentRect = new Rect(0f, 48f, inRect.width, inRect.height - 48f - 44f);
        Widgets.DrawMenuSection(contentRect);
        Rect innerRect = contentRect.ContractedBy(12f);

        float textHeight = Text.CalcHeight(bodyText, innerRect.width - 16f);
        Rect viewRect = new Rect(0f, 0f, innerRect.width - 16f, textHeight);

        GUI.color = terminalColor;
        Widgets.BeginScrollView(innerRect, ref scrollPosition, viewRect);
        Widgets.Label(new Rect(0f, 0f, viewRect.width, textHeight), bodyText);
        Widgets.EndScrollView();
        GUI.color = Color.white;

        Rect buttonRect = new Rect(inRect.width - 140f, inRect.height - 35f, 140f, 35f);
        if (Widgets.ButtonText(buttonRect, "FCP_TerminalLetter_Continue".Translate()))
        {
            closeSound?.PlayOneShotOnCamera();
            Close();
        }
    }
}
