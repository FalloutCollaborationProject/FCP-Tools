using HarmonyLib;

namespace FCP.Compatibility.VNPE;

[UsedImplicitly, StaticConstructorOnStartup]
public static class HarmonyStarter
{
    static HarmonyStarter()
    {
        Log.Message("VNPE harmony starter started");
        var harmony = new Harmony("FCP.VNPEPatch");
        harmony.PatchAll();
    }
}