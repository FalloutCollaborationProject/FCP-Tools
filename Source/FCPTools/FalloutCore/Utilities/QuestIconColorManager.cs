using UnityEngine;

namespace FCP.Core;

public static class QuestIconColorManager
{
    private static readonly Color GreenColor = new Color(51f / 255f, 1f, 51f / 255f);
    private static readonly Color AmberColor = new Color(1f, 191f / 255f, 38f / 255f);

    public static Color GetColor()
    {
        var theme = FCPCoreMod.Settings?.General?.questIconColorTheme ?? TerminalColorTheme.Green;
        return theme == TerminalColorTheme.Amber ? AmberColor : GreenColor;
    }
}
