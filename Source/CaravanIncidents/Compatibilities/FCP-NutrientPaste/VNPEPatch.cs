using FCP_RadiantQuests;
using HarmonyLib;
using Mono.Unix.Native;
using PipeSystem;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FCP_NutrientPaste
{
    [StaticConstructorOnStartup]
    public static class HarmonyStarter
    {
        static HarmonyStarter()
        {
            Log.Message("VNPE harmony starter started");
            Harmony harmony = new Harmony("FCP.VNPEPatch");
            harmony.PatchAll();
        }
    }
    [HarmonyPatch]
    public static class CompAnimalCagePatch
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(CompAnimalCage), nameof(CompAnimalCage.CompTick));
        }

        private static void Postfix(CompAnimalCage __instance)
        {
            //Log.Message("Postfix");
            foreach (CompResource comp in __instance.parent.GetComps<CompResource>())
            {

                if ((((Def)(object)comp.Props?.pipeNet)?.defName) == "VNPE_NutrientPasteNet")
                {
                    if (__instance.parent.IsHashIntervalTick(600) && comp != null && comp.PipeNet.Stored > 1f && __instance.FuelPercentOfMax < 0.5f)
                    {
                        comp.PipeNet.DrawAmongStorage(1f, comp.PipeNet.storages);
                        __instance.Refuel(18f);
                        return;
                    }
                }
            }
        }
    }
}
