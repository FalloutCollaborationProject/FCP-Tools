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

namespace FCP.Core.RadiantQuests.HarmonyPatches
{
    public class FloatMenuProvider_CarryToCage : FloatMenuOptionProvider
    {
        protected override bool Drafted => true;

        protected override bool Undrafted => true;

        protected override bool Multiselect => false;

        protected override FloatMenuOption GetSingleOptionFor(Thing clickedThing, FloatMenuContext context)
        {
            var targ = (LocalTargetInfo)clickedThing;
            if (Validator(targ))
            {
                var target = clickedThing as Pawn;
                var pawn = context.FirstSelectedPawn;
                if (!pawn.CanReserveAndReach(target, PathEndMode.OnCell, Danger.Deadly, 1, -1, null, ignoreOtherReservations: true))
                {
                    return null;
                }
                string text = "";
                Action action = null;
                if (!AnimalCageUtility.TryGetCagesThatFitBodySize(target.RaceProps.baseBodySize, target.Map, out List<CompAnimalCage> cages))
                {
                    return null;
                }
                foreach (CompAnimalCage item in cages)
                {
                    text = "FCP_CarryAnimalToCage".Translate(target);
                    if (target.IsQuestLodger())
                    {
                        text += " (" + "CryptosleepCasketGuestsNotAllowed".Translate() + ")";
                        continue;
                    }
                    if (!item.CanAcceptPawn(target))
                    {
                        text += " (" + "CryptosleepCasketOccupied".Translate() + ")";
                        continue;
                    }
                    Thing pod = item.parent;
                    action = delegate
                    {
                        Job job = JobMaker.MakeJob(DefOfs.FCP_CarryAnimalToCage, target, pod);
                        job.count = 1;
                        pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                    };
                    break;
                }
                if (!text.NullOrEmpty())
                {
                    return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(text, action), pawn, target);
                }
            }
            return null;
        }

        private static bool Validator(LocalTargetInfo targ)
        {
            if (!targ.HasThing)
            {
                return false;
            }
            if (!(targ.Thing is Pawn pawn) || !pawn.AnimalOrWildMan() || pawn.Dead)
            {
                return false;
            }
            if (pawn.Faction != Faction.OfPlayer && !pawn.Downed)
            {
                return false;
            }
            return true;
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

