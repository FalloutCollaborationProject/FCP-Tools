using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Diagnostics;
using Verse;
using Verse.AI;

namespace FCP_RadiantQuests.HarmonyPatches
{

    [HarmonyPatch]
    public static class RightClickAnimalPatch
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(FloatMenuMakerMap), "AddHumanlikeOrders");
        }
        private static void Postfix(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
        {
            foreach (LocalTargetInfo item in GenUI.TargetsAt(clickPos, new TargetingParameters
            {
                canTargetPawns = true,
                canTargetBuildings = false,
                mapObjectTargetsMustBeAutoAttackable = false,
                validator = delegate (TargetInfo targ)
                {
                    if (!targ.HasThing)
                    {
                        return false;
                    }
                    if (!(targ.Thing is Pawn pawn) || !pawn.AnimalOrWildMan() || pawn.Dead)
                    {
                        return false;
                    }
                    if(pawn.Faction != Faction.OfPlayer && !pawn.Downed)
                    {
                        return false;
                    }
                    return true;
                }
            }))
            {
                Pawn pawn2 = item.Pawn;
                if (pawn2 != pawn)
                {
                    CompAnimalCage.AddCarryToCageJobs(opts, pawn, pawn2);
                }
            }

        }
    }
    [HarmonyPatch]
    public static class WillingToAnimalsInCagePatch
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(Pawn_TraderTracker), nameof(Pawn_TraderTracker.ColonyThingsWillingToBuy));
        }

        private static void Postfix(ref IEnumerable<Thing> __result, Pawn_TraderTracker __instance, Pawn ___pawn)
        {
            List<Thing> list = __result.ToList();
            if (ModsConfig.AnomalyActive)
            {
                List<Building> list1 = ___pawn.Map.listerBuildings.allBuildingsColonist.Where(c => c.HasComp<CompAnimalCage>()).ToList();
                foreach (Building item in list1)
                {
                    //Log.Message(item.Label);
                    CompAnimalCage comp = item.GetComp<CompAnimalCage>();
                    if (comp.Occupant != null)
                    {
                        //Log.Message(comp.HeldPawn.Label);
                        list.Add(comp.Occupant);
                    }
                }
            }
            __result = list;
        }
    }
    [HarmonyPatch]
    public static class CageTradeDealPatch
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(TradeDeal), "InSellablePosition");
        }

        private static void Postfix(ref bool __result, TradeDeal __instance, Thing t, out string reason)
        {
            //Log.Message(t.ParentHolder);
            if (!__result && !t.Spawned && t.holdingOwner != null &&  ((Pawn)t).health.capacities.CanBeAwake && !((Pawn)t).DeadOrDowned)
            {
                __result = true;
            }
            reason = null;
        }
    }
}
