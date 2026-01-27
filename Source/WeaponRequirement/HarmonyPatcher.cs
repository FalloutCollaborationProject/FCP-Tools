using HarmonyLib;

namespace FCP.WeaponRequirement;

[StaticConstructorOnStartup]
public static class HarmonyPatcher
{
    static HarmonyPatcher()
    {
        new Harmony("FCP.WeaponRequirement").PatchAll();
    }
}