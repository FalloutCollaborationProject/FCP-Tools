using System.Linq;
using UnityEngine;

namespace FCP.Core;

public class Dialog_SetVaultNumber : Window
{
    private readonly CompVaultNumberStencil stencil;
    private string editBuffer;

    public override Vector2 InitialSize => new Vector2(280f, 200f);

    public Dialog_SetVaultNumber(CompVaultNumberStencil stencil)
    {
        this.stencil = stencil;
        editBuffer = stencil.Number ?? string.Empty;
        forcePause = true;
        doCloseX = true;
        absorbInputAroundWindow = true;
    }

    public override void DoWindowContents(Rect inRect)
    {
        Color terminalColor = TerminalColors.PrimaryColor;

        Text.Font = GameFont.Medium;
        GUI.color = terminalColor;
        Widgets.Label(new Rect(0f, 0f, inRect.width, 35f), "FCP_VaultNumberStencil_DialogTitle".Translate());
        Text.Font = GameFont.Small;
        Widgets.DrawLineHorizontal(0f, 38f, inRect.width);
        GUI.color = Color.white;

        GUI.color = terminalColor;
        string typed = Widgets.TextField(new Rect(0f, 55f, inRect.width, 35f), editBuffer);
        GUI.color = Color.white;

        typed = new string(typed.Where(char.IsDigit).ToArray());
        if (typed.Length > stencil.Props.digitCount)
            typed = typed.Substring(0, stencil.Props.digitCount);
        editBuffer = typed;

        bool valid = editBuffer.Length >= 1 && editBuffer.Length <= stencil.Props.digitCount;
        if (!valid)
        {
            GUI.color = Color.red;
            Widgets.Label(new Rect(0f, 92f, inRect.width, 25f), "FCP_VaultNumberStencil_DialogNeedsDigits".Translate(stencil.Props.digitCount));
            GUI.color = Color.white;
        }

        GUI.color = valid ? terminalColor : new Color(terminalColor.r, terminalColor.g, terminalColor.b, 0.5f);
        if (Widgets.ButtonText(new Rect(0f, inRect.height - 35f, inRect.width, 35f), "Confirm".Translate()) && valid)
        {
            stencil.SetNumber(editBuffer);
            Close();
        }
        GUI.color = Color.white;
    }
}
