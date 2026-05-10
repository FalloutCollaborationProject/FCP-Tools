using HarmonyLib;

namespace FCP.Core;

[StaticConstructorOnStartup]
public static class HomingProjectiles_HarmonyInit
{
    static HomingProjectiles_HarmonyInit()
    {
        new HarmonyLib.Harmony("FCP.Core.HomingProjectiles").PatchAll();
    }
}
