using HarmonyLib;
using Verse;

namespace FCP.Core.Backpacks;

[StaticConstructorOnStartup]
public static class BackpacksHarmony
{
    static BackpacksHarmony()
    {
        new Harmony("FCP.Core.Backpacks").PatchAll();
    }
}
