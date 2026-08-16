namespace FCP.Core;

public class ScenPart_TerminalStartDialog : ScenPart
{
    public string title = "Transmission";
    public string text;
    public SoundDef closeSound;

    public override void PostGameStart()
    {
        base.PostGameStart();

        string resolvedText = text;
        if (!resolvedText.NullOrEmpty() && resolvedText.Contains("[FCP_Year]"))
        {
            int year = FCPCoreMod.SettingsTab<ScenarioSettings>()?.startingYear ?? 2287;
            resolvedText = resolvedText.Replace("[FCP_Year]", year.ToString());
        }

        Find.WindowStack.Add(new Dialog_TerminalStartLetter(title, resolvedText, closeSound));
    }

    public override string Summary(Scenario scen)
    {
        return "FCP_ScenPart_TerminalStartDialog".Translate();
    }
}
