using System.Collections.Generic;
using RimWorld;
using Verse;

namespace FCP.Core;

public class ComplexThreatWorker_SleepingRadroaches : ComplexThreatWorker_SleepingThreat
{
    private static readonly HashSet<string> RadroachKindDefNames = new HashSet<string>
    {
        "FCP_Animal_Radroach",
        "FCP_Animal_Glowing_Radroach",
    };

    protected override bool CanResolveInt(ComplexResolveParams parms)
    {
        if (base.CanResolveInt(parms))
        {
            if (parms.hostileFaction != null)
                return parms.hostileFaction == Faction.OfInsects;
            return true;
        }
        return false;
    }

    protected override IEnumerable<PawnKindDef> GetPawnKindsForPoints(float points)
    {
        return PawnUtility.GetCombatPawnKindsForPoints((PawnKindDef k) => RadroachKindDefNames.Contains(k.defName), points);
    }
}
