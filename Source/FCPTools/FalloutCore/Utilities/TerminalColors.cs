using UnityEngine;

namespace FCP.Core;

public static class TerminalColors
{
    private static readonly Color GreenPrimary = new Color(0.2f, 1f, 0.2f);
    private static readonly Color GreenHighlight = new Color(0.4f, 1f, 0.4f);
    
    private static readonly Color AmberPrimary = new Color(1f, 0.75f, 0.15f);
    private static readonly Color AmberHighlight = new Color(1f, 0.85f, 0.4f);

    public static Color PrimaryColor
    {
        get
        {
            var theme = FCPCoreMod.Settings?.General?.terminalColorTheme ?? TerminalColorTheme.Green;
            return theme == TerminalColorTheme.Amber ? AmberPrimary : GreenPrimary;
        }
    }

    public static Color HighlightColor
    {
        get
        {
            var theme = FCPCoreMod.Settings?.General?.terminalColorTheme ?? TerminalColorTheme.Green;
            return theme == TerminalColorTheme.Amber ? AmberHighlight : GreenHighlight;
        }
    }
}
