using HarmonyLib;

namespace FCP.Core;

public class FCPCoreMod : Mod
{
    public static FCPCoreMod mod;
    public static Harmony harmony;

    public const string LatePatchesCategory = "FCP.Core.LatePatches";

    public FCPCoreMod(ModContentPack content) : base(content)
    {
        harmony = new Harmony("FCP.Core.Patches"); // PatchesUwU ~ Steve
        harmony.PatchAllUncategorized();
        LongEventHandler.ExecuteWhenFinished(() =>
        {
            harmony.PatchCategory(LatePatchesCategory);
        });
        mod = this;
        FCPLog.Warning("Beta version: bugs likely, if not guaranteed! " +
                       "Report bugs on steam workshop page or on discord: 3HEXN3Qbn4");
    }
}
