using HarmonyLib;

namespace FCP.Core;

[StaticConstructorOnStartup]
public static class WeaponRequirement_HarmonyInit
{
    static WeaponRequirement_HarmonyInit()
    {
        new HarmonyLib.Harmony("FCP.Core.WeaponRequirement").PatchAll();
    }
}
