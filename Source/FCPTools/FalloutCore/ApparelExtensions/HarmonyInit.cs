using HarmonyLib;
using Verse;

namespace FCP.Core;

[StaticConstructorOnStartup]
public static class ApparelExtensions_HarmonyInit
{
    static ApparelExtensions_HarmonyInit()
    {
        new HarmonyLib.Harmony("FCP.Core.ApparelExtension").PatchAll();
    }
}
