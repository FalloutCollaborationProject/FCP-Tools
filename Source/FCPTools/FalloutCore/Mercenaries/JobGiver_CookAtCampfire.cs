using Verse;
using Verse.AI;
using RimWorld;
using System.Linq;
using UnityEngine;
using HarmonyLib;
using System.Reflection;

namespace FCP.Core
{
    public class JobGiver_CookAtCampfire : ThinkNode_JobGiver
    {
        private const float MaxSearchRadius = 25f;
        private static MethodInfo tryFindBestBillIngredientsMI = null;

        public override void ResolveReferences()
        {
            base.ResolveReferences();
            tryFindBestBillIngredientsMI = AccessTools.Method(typeof(WorkGiver_DoBill), "TryFindBestBillIngredients", new System.Type[] { typeof(Bill), typeof(Pawn), typeof(Thing), typeof(List<ThingCount>), typeof(List<IngredientCount>) });
            if (tryFindBestBillIngredientsMI == null)
            {
                Log.ErrorOnce("FCP: Could not find MethodInfo for WorkGiver_DoBill.TryFindBestBillIngredients via reflection.", 984653);
            }
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn.Downed || !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) || pawn.WorkTagIsDisabled(WorkTags.Cooking))
            {
                return null;
            }
            if (FCPDefOf.CookMeal_Simple == null)
            {
                Log.ErrorOnce("FCP: FCPDefOf.CookMeal_Simple is null.", 984651);
                return null;
            }

            Thing campfire = FindClosestCampfire(pawn);
            if (campfire != null)
            {
                Bill bill = FindOrCreateSimpleMealBill(campfire, FCPDefOf.CookMeal_Simple);
                if (bill != null && tryFindBestBillIngredientsMI != null)
                {
                    List<ThingCount> chosenIngredients = new List<ThingCount>();
                    List<IngredientCount> missingIngredients = null;
                    object[] parameters = new object[] { bill, pawn, campfire, chosenIngredients, missingIngredients };
                    bool ingredientsFound = (bool)tryFindBestBillIngredientsMI.Invoke(null, parameters);

                    if (ingredientsFound)
                    {
                        Job cookJob = JobMaker.MakeJob(JobDefOf.DoBill, campfire);
                        cookJob.bill = bill;
                        return cookJob;
                    }
                }
            }
            Thing butcherSpot = FindClosestButcherSpot(pawn);
            if (butcherSpot != null)
            {
                Corpse corpse = FindClosestButcherableCorpse(pawn, butcherSpot.Position, MaxSearchRadius);
                if (corpse != null)
                {
                    if (FCPDefOf.ButcherCorpseFlesh == null)
                    {
                        Log.ErrorOnce("FCP: FCPDefOf.ButcherCorpseFlesh is null.", 984652);
                        return null;
                    }
                    Bill butcherBill = FindOrCreateButcherBill(butcherSpot, FCPDefOf.ButcherCorpseFlesh);
                    if (butcherBill != null)
                    {
                        Job butcherJob = JobMaker.MakeJob(JobDefOf.DoBill, butcherSpot);
                        butcherJob.bill = butcherBill;
                        return butcherJob;
                    }
                }
            }
            return null;
        }

        private Thing FindClosestCampfire(Pawn pawn)
        {
            return GenClosest.ClosestThingReachable(
                pawn.Position,
                pawn.Map,
                ThingRequest.ForDef(ThingDefOf.Campfire),
                PathEndMode.InteractionCell,
                TraverseParms.For(pawn, Danger.Some, TraverseMode.ByPawn),
                MaxSearchRadius,
                (Thing t) => t.Faction == pawn.Faction || t.Faction == null &&
                             t.TryGetComp<CompRefuelable>()?.HasFuel == true &&
                             pawn.CanReserveAndReach(t, PathEndMode.InteractionCell, Danger.Some)
            );
        }

        private Thing FindClosestButcherSpot(Pawn pawn)
        {
            return GenClosest.ClosestThingReachable(
                pawn.Position,
                pawn.Map,
                ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial),
                PathEndMode.InteractionCell,
                TraverseParms.For(pawn, Danger.Some, TraverseMode.ByPawn),
                MaxSearchRadius,
                (Thing t) => (t.def == DefDatabase<ThingDef>.GetNamed("ButcherSpot") || t.def == DefDatabase<ThingDef>.GetNamed("ButcherTable")) &&
                             (t.Faction == pawn.Faction || t.Faction == null) &&
                             pawn.CanReserveAndReach(t, PathEndMode.InteractionCell, Danger.Some)
            );
        }

        private Corpse FindClosestButcherableCorpse(Pawn pawn, IntVec3 center, float radius)
        {
            Map map = pawn.Map;
            System.Predicate<Thing> validator = (Thing t) =>
            {
                Corpse c = t as Corpse;
                return c != null &&
                       !c.IsForbidden(pawn) &&
                       c.InnerPawn.RaceProps.IsFlesh &&
                       !c.InnerPawn.RaceProps.IsMechanoid &&
                       c.GetRotStage() == RotStage.Fresh &&
                       pawn.CanReserveAndReach(c, PathEndMode.ClosestTouch, Danger.Some);
            };

            Thing corpseThing = GenClosest.ClosestThingReachable(
                center, map, ThingRequest.ForGroup(ThingRequestGroup.Corpse),
                PathEndMode.ClosestTouch, TraverseParms.For(pawn), radius, validator);

            return corpseThing as Corpse;

        }



        private Bill FindOrCreateSimpleMealBill(Thing campfire, RecipeDef recipeDef)
        {
            IBillGiver billGiver = campfire as IBillGiver;
            if (billGiver == null) return null;
            foreach (Bill bill in billGiver.BillStack)
            {
                if (bill.recipe == recipeDef && !bill.suspended)
                {
                    return bill;
                }
            }
            if (!recipeDef.AvailableOnNow(campfire)) return null;

            Bill_Production newBill = new Bill_Production(recipeDef);
            newBill.repeatMode = BillRepeatModeDefOf.RepeatCount;
            newBill.repeatCount = 1;
            newBill.allowedSkillRange = new IntRange(0, 20);

            billGiver.BillStack.AddBill(newBill);
            return newBill;
        }

        private Bill FindOrCreateButcherBill(Thing butcherStation, RecipeDef recipeDef)
        {
            IBillGiver billGiver = butcherStation as IBillGiver;
            if (billGiver == null || recipeDef == null) return null;
            foreach (Bill bill in billGiver.BillStack)
            {
                if (bill.recipe == recipeDef && !bill.suspended)
                {
                    return bill;
                }
            }
            if (!recipeDef.AvailableOnNow(butcherStation)) return null;

            Bill_Production newBill = new Bill_Production(recipeDef);
            newBill.repeatMode = BillRepeatModeDefOf.Forever;
            newBill.allowedSkillRange = new IntRange(0, 20);

            billGiver.BillStack.AddBill(newBill);
            return newBill;
        }
    }
}