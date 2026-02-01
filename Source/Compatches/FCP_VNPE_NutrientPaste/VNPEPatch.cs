using FCP_RadiantQuests;
using HarmonyLib;
using System.Reflection;
using PipeSystem;

namespace FCP.Compatibility.VNPE;

[HarmonyPatch]
public static class CompAnimalCagePatch
{
    private static MethodBase TargetMethod()
    {
        return AccessTools.Method(typeof(CompAnimalCage), nameof(CompAnimalCage.CompTick));
    }

    // ReSharper disable once InconsistentNaming
    private static void Postfix(CompAnimalCage __instance)
    {
        foreach (CompResource comp in __instance.parent.GetComps<CompResource>())
        {
            if (comp.Props?.pipeNet?.defName != "VNPE_NutrientPasteNet")
                continue;
            if (!__instance.parent.IsHashIntervalTick(600))
                continue;
                
            if (comp.PipeNet.Stored > 1f && __instance.FuelPercentOfMax < 0.5f)
            {
                comp.PipeNet.DrawAmongStorage(1f, comp.PipeNet.storages);
                __instance.Refuel(18f);
                return;
            }
        }
    }
}

