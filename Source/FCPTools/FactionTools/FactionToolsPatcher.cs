using HarmonyLib;
using Verse;

namespace FCP.Factions;

[StaticConstructorOnStartup]
public class FactionToolsPatcher
{
    static FactionToolsPatcher()
    {
        var harmony = new Harmony("FCP.FactionTools");
        harmony.PatchAll();
    }
}