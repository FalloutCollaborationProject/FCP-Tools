using HarmonyLib;

namespace FCP;

[StaticConstructorOnStartup]
public class FactionToolsPatcher
{
    static FactionToolsPatcher()
    {
        var harmony = new Harmony("com.FCP.FactionTools");
        harmony.PatchAll();
    }
}