using HarmonyLib;

namespace FCP.Core;

[StaticConstructorOnStartup]
public static class Gooification_HarmonyInit
{
    static Gooification_HarmonyInit()
    {
        new HarmonyLib.Harmony("FCP.Core.Gooification").PatchAll();
    }
}
