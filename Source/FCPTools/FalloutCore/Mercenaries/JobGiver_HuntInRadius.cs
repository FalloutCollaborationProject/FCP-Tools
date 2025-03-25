using Verse;
using Verse.AI;
using RimWorld;
using System.Linq;
using UnityEngine;
using Verse.AI.Group;

namespace FCP.Core
{
    public class JobGiver_HuntInRadius : ThinkNode_JobGiver
    {

        protected override Job TryGiveJob(Pawn pawn)
        {
            var lordJob = pawn.GetLord()?.LordJob as LordJob_MercenaryCamp;
            if (lordJob != null && lordJob.CampHasEnoughFood(3))
            {
                return null;
            }

            if (pawn.Downed || !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Moving) || !pawn.CanReserveAndReach(pawn.Position, PathEndMode.OnCell, Danger.Deadly))
            {
                return null;
            }

            PawnDuty duty = pawn.mindState.duty;
            if (duty == null) return null;
            IntVec3 center = duty.focus.Cell;
            float radius = duty.radius;
            if (!center.IsValid || radius <= 0) return null;
            if (pawn.WorkTagIsDisabled(WorkTags.Violent) || pawn.WorkTagIsDisabled(WorkTags.Hunting))
            {
                return null;
            }

            float searchRadius = radius;
            System.Predicate<Thing> validator = (Thing t) =>
            {
                Pawn animal = t as Pawn;
                return animal != null &&
                       animal.AnimalOrWildMan() &&
                       !animal.IsFighting() &&
                       !animal.IsBurning() &&
                       animal.Faction == null &&
                       animal.BodySize <= pawn.BodySize * 1.5f &&
                       !animal.RaceProps.predator &&
                       pawn.CanReserveAndReach(animal, PathEndMode.Touch, Danger.Deadly);
            };

            Thing bestPrey = GenClosest.ClosestThingReachable(
                center,
                pawn.Map,
                ThingRequest.ForGroup(ThingRequestGroup.Pawn),
                PathEndMode.Touch,
                TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn),
                searchRadius,
                validator
            );

            if (bestPrey != null)
            {
                return JobMaker.MakeJob(JobDefOf.Hunt, bestPrey);
            }
            return null;
        }
    }
}