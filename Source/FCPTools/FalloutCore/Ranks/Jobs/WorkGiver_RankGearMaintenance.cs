using FCP.Core;
using FCP.Core.Access;
using FCP.Core.PowerArmor;
using Verse.AI;

namespace FCP.Ranks.Jobs;

public class WorkGiver_RankGearMaintenance : WorkGiver_Scanner
{
    private List<ThingCount> chosenResources = new List<ThingCount>();

    public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial);

    public override PathEndMode PathEndMode => PathEndMode.Touch;

    public override Danger MaxPathDanger(Pawn pawn)
    {
        return Danger.Deadly;
    }

    private static bool IsEligibleSquire(Pawn pawn)
    {
        PawnRankData data = Current.Game.GetComponent<GameComponent_PawnRanks>()?.GetRankData(pawn);
        return data?.mentor != null && !data.mentor.Dead && data.track != null && data.track.allowsGearMaintenance
            && RankTrackUtility.IsFactionActive(data.track);
    }

    private List<ThingDefFloatClass> GetRepairResources(CompPowerArmorStation stationComp)
    {
        List<ThingDefFloatClass> repairResources = new List<ThingDefFloatClass>();

        foreach (Apparel apparel in stationComp.HeldApparels)
        {
            if (apparel.HitPoints < apparel.MaxHitPoints)
            {
                var repairComp = apparel.GetComp<CompRepairableAtStation>();
                if (repairComp != null && repairComp.Props.repairResourcesPerHP != null && repairComp.Props.repairResourcesPerHP.Count > 0)
                {
                    repairResources.AddRange(repairComp.Props.repairResourcesPerHP);
                    break;
                }
            }
        }

        return repairResources;
    }

    private bool TryFindRepairResources(List<ThingDefFloatClass> repairResources, Pawn pawn, Thing building, List<ThingCount> chosen)
    {
        List<IngredientCount> ingredients = repairResources.Select((ThingDefFloatClass tc) => tc.ToIngredientCount()).ToList();

        IntVec3 rootCell = building is Building b && b.def.hasInteractionCell ? b.InteractionCell : building.Position;
        return WorkGiver_DoBill_Access.TryFindBestIngredientsHelper(delegate (Thing t)
        {
            foreach (IngredientCount ingredient in ingredients)
            {
                if (ingredient.filter.Allows(t))
                    return true;
            }
            return false;
        }, (List<Thing> foundThings) => WorkGiver_DoBill_Access.TryFindBestIngredientsInSet_NoMixHelper(foundThings, ingredients, chosen, rootCell, alreadySorted: false, null), ingredients, pawn, building, chosen, 999f);
    }

    public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        if (!IsEligibleSquire(pawn))
            return false;

        if (t is not Building building)
            return false;

        var stationComp = building.GetComp<CompPowerArmorStation>();
        if (stationComp == null || !stationComp.HeldApparels.Any())
            return false;

        List<ThingDefFloatClass> repairResources = GetRepairResources(stationComp);
        if (repairResources.Count == 0)
            return false;

        if (!pawn.CanReserve(building, 1, -1, null, forced))
            return false;

        chosenResources.Clear();
        return TryFindRepairResources(repairResources, pawn, building, chosenResources);
    }

    public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        Building building = (Building)t;
        var stationComp = building.GetComp<CompPowerArmorStation>();

        List<ThingDefFloatClass> repairResources = GetRepairResources(stationComp);
        if (repairResources.Count == 0)
            return null;

        chosenResources.Clear();
        if (!TryFindRepairResources(repairResources, pawn, building, chosenResources))
            return null;

        Job job = JobMaker.MakeJob(RankJobDefOf.FCP_Job_RankGearMaintenance, building);
        job.targetQueueB = new List<LocalTargetInfo>(chosenResources.Count);
        job.countQueue = new List<int>(chosenResources.Count);

        foreach (ThingCount chosenResource in chosenResources)
        {
            job.targetQueueB.Add(chosenResource.Thing);
            job.countQueue.Add(chosenResource.Count);
        }

        job.haulMode = HaulMode.ToCellNonStorage;
        return job;
    }
}
