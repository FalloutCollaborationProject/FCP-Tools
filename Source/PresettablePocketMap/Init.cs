using HarmonyLib;

namespace FCP.PocketMaps
{
    [StaticConstructorOnStartup]
    public static class Init
    {
        static Init()
        {
            new Harmony("fcp.pocketmaps").PatchAll();
        }
    }
}
