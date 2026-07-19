using System.Collections.Generic;
using RimWorld;
using Verse;

namespace FCP.Core.Robotics;

public class CompAssignableToPawn_RobotBed : CompAssignableToPawn_Bed
{
    public override IEnumerable<Pawn> AssigningCandidates
    {
        get
        {
            Map map = parent?.Map;
            if (map == null)
            {
                yield break;
            }

            foreach (Pawn pawn in map.mapPawns.AllPawnsSpawned)
            {
                if (pawn.Faction == Faction.OfPlayer && RobotUtility.IsAnyRobot(pawn))
                {
                    yield return pawn;
                }
            }
        }
    }

    public override AcceptanceReport CanAssignTo(Pawn pawn)
    {
        if (pawn == null || pawn.Faction != Faction.OfPlayer || !RobotUtility.IsAnyRobot(pawn))
        {
            return false;
        }

        return true;
    }
}
