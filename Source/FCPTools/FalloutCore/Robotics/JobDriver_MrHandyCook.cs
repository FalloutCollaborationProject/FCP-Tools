using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace FCP.Core.Robotics
{
    public static class MrHandyCookUtility
    {
        public const int RawFoodNeeded = 10;
        private const float SearchRadius = 8f;

        private static readonly string[] CookingStationDefNames = { "ElectricStove", "FueledStove", "CookingSpot", "Campfire" };

        private static ThingCategoryDef foodRawCategory;
        private static ThingDef mealSimpleDef;

        private static ThingCategoryDef FoodRawCategory => foodRawCategory ??= DefDatabase<ThingCategoryDef>.GetNamed("FoodRaw");
        public static ThingDef MealSimpleDef => mealSimpleDef ??= DefDatabase<ThingDef>.GetNamed("MealSimple");

        public static bool IsCookingStation(Thing t)
        {
            return t is Building && CookingStationDefNames.Contains(t.def.defName);
        }

        public static bool IsRawFood(Thing t)
        {
            return t.def.IsWithinCategory(FoodRawCategory);
        }

        private static List<Thing> FindNearbyRawFood(Thing stove)
        {
            return GenRadial.RadialDistinctThingsAround(stove.Position, stove.Map, SearchRadius, true)
                .Where(t => IsRawFood(t) && !t.IsForbidden(Faction.OfPlayer))
                .ToList();
        }

        public static bool HasEnoughRawFoodNear(Thing stove, int needed)
        {
            return FindNearbyRawFood(stove).Sum(t => t.stackCount) >= needed;
        }

        public static bool TryConsumeRawFoodNear(Thing stove, int needed)
        {
            List<Thing> candidates = FindNearbyRawFood(stove);
            if (candidates.Sum(t => t.stackCount) < needed)
            {
                return false;
            }

            int remaining = needed;
            foreach (Thing candidate in candidates)
            {
                if (remaining <= 0)
                {
                    break;
                }

                int take = System.Math.Min(remaining, candidate.stackCount);
                candidate.SplitOff(take).Destroy();
                remaining -= take;
            }

            return true;
        }
    }

    public class JobDriver_MrHandyCook : JobDriver
    {
        private const int WorkTicks = 400;

        private Thing Stove => job.GetTarget(TargetIndex.A).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(Stove, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOn(() => !MrHandyCookUtility.HasEnoughRawFoodNear(Stove, MrHandyCookUtility.RawFoodNeeded));

            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);

            Toil work = Toils_General.Wait(WorkTicks);
            work.WithProgressBarToilDelay(TargetIndex.A);
            work.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
            yield return work;

            yield return new Toil
            {
                initAction = FinishCooking,
                defaultCompleteMode = ToilCompleteMode.Instant,
            };
        }

        private void FinishCooking()
        {
            if (!MrHandyCookUtility.TryConsumeRawFoodNear(Stove, MrHandyCookUtility.RawFoodNeeded))
            {
                return;
            }

            Thing meal = ThingMaker.MakeThing(MrHandyCookUtility.MealSimpleDef);
            meal.stackCount = 1;
            GenPlace.TryPlaceThing(meal, Stove.Position, Stove.Map, ThingPlaceMode.Near);
        }
    }
}
