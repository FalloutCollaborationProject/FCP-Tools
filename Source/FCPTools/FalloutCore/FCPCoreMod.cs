using HarmonyLib;

namespace FCP.Core;

public class FCPCoreMod : Mod
{
    public FCPCoreMod(ModContentPack content) : base(content)
    {
        var harmony = new Harmony("FCP.Core");
        harmony.PatchAllUncategorized();
    }
}