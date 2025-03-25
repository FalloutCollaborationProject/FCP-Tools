using Verse;
using Verse.AI;
using RimWorld;
using System.Linq;
using UnityEngine;
using Verse.AI.Group;

namespace FCP.Core
{
    public class JobGiver_ForageInRadius : ThinkNode_JobGiver
    {

        protected override Job TryGiveJob(Pawn pawn)
        {
            var lordJob = pawn.GetLord()?.LordJob as LordJob_MercenaryCamp;
            if (lordJob != null && lordJob.CampHasEnoughFood(3))
            {
                return null;
            }

            if (pawn.Downed || !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) || !pawn.CanReserveAndReach(pawn.Position, PathEndMode.OnCell, Danger.Some))
            {
                return null;
            }

            PawnDuty duty = pawn.mindState.duty;
            if (duty == null) return null;
            IntVec3 center = duty.focus.Cell;
            float radius = duty.radius;
            if (!center.IsValid || radius <= 0) return null;

            if (pawn.WorkTagIsDisabled(WorkTags.PlantWork))
            {
                return null;
            }
            float searchRadius = radius;

            System.Predicate<Thing> validator = (Thing t) =>
            {
                Plant plant = t as Plant;
                return plant != null &&
                       plant.Spawned &&
                       !plant.IsForbidden(pawn) &&
                       plant.HarvestableNow &&
                       pawn.CanReserveAndReach(plant, PathEndMode.Touch, Danger.Some);
            };

            Thing bestPlant = GenClosest.ClosestThingReachable(
                center,
                pawn.Map,
                ThingRequest.ForGroup(ThingRequestGroup.Plant),
                PathEndMode.Touch,
                TraverseParms.For(pawn, Danger.Some, TraverseMode.ByPawn),
                searchRadius,
                validator
            );

            if (bestPlant != null)
            {
                return JobMaker.MakeJob(JobDefOf.Harvest, bestPlant);
            }
            return null;
        }
    }
}