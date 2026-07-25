using HarmonyLib;
using RimWorld;
// ReSharper disable InconsistentNaming

namespace FCP.Core;

[HarmonyPatch(typeof(ScenPart_GameStartDialog), nameof(ScenPart_GameStartDialog.PostGameStart))]
public static class Patch_ScenPart_GameStartDialog_StartingYear
{
    public static void Prefix(ScenPart_GameStartDialog __instance)
    {
        Traverse textField = Traverse.Create(__instance).Field("text");
        string text = textField.GetValue<string>();
        if (text.NullOrEmpty() || !text.Contains("[FCP_Year]"))
        {
            return;
        }

        int year = FCPCoreMod.SettingsTab<ScenarioSettings>()?.startingYear ?? 2287;
        textField.SetValue(text.Replace("[FCP_Year]", year.ToString()));
    }
}
