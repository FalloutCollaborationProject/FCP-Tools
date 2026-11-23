using UnityEngine;
using Verse.Sound;

namespace FCP.Synths;

public class PawnsArrivalModeWorker_InstituteRelay : PawnsArrivalModeWorker
{
    private static readonly Color RelayBlue = new(0.35f, 0.65f, 1f);

    public override void Arrive(List<Pawn> pawns, IncidentParms parms)
    {
        Map map = (Map)parms.target;

        foreach (Pawn pawn in pawns)
        {
            IntVec3 loc = CellFinder.RandomClosewalkCellNear(parms.spawnCenter, map, 6);
            PlayRelayEffect(loc, map);
            GenSpawn.Spawn(pawn, loc, map, parms.spawnRotation);
        }
    }

    public override bool TryResolveRaidSpawnCenter(IncidentParms parms)
    {
        Map map = (Map)parms.target;
        parms.spawnRotation = Rot4.Random;

        if (!parms.spawnCenter.IsValid && !DropCellFinder.TryFindRaidDropCenterClose(out parms.spawnCenter, map))
        {
            parms.raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn;
            return parms.raidArrivalMode.Worker.TryResolveRaidSpawnCenter(parms);
        }

        return true;
    }

    private static void PlayRelayEffect(IntVec3 cell, Map map)
    {
        Vector3 loc = cell.ToVector3Shifted();

        FleckCreationData flash = FleckMaker.GetDataStatic(loc, map, FleckDefOf.PsycastSkipFlashEntry);
        flash.instanceColor = RelayBlue;
        map.flecks.CreateFleck(flash);

        FleckCreationData inner = FleckMaker.GetDataStatic(loc, map, FleckDefOf.PsycastSkipInnerExit);
        inner.instanceColor = RelayBlue;
        map.flecks.CreateFleck(inner);

        FleckCreationData ring = FleckMaker.GetDataStatic(loc, map, FleckDefOf.PsycastSkipOuterRingExit);
        ring.instanceColor = RelayBlue;
        map.flecks.CreateFleck(ring);

        SoundDefOf.Psycast_Skip_Entry.PlayOneShot(new TargetInfo(cell, map));
    }
}
