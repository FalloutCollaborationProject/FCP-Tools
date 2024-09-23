using HarmonyLib;

namespace FCP.Tools;

public class FCPToolsMod : Mod
{
    public FCPToolsMod(ModContentPack content) : base(content)
    {
        var harmony = new Harmony("com.FCPTools.Patches");
        harmony.PatchAll();
        
    }
}