using System.Collections.Generic;
using RimWorld;
using Verse;

namespace FCP.Core.Ghouls;

public class ComplexThreatWorker_SleepingFeralGhouls : ComplexThreatWorker_SleepingThreat
{
    protected override bool CanResolveInt(ComplexResolveParams parms)
    {
        if (base.CanResolveInt(parms))
        {
            if (parms.hostileFaction != null)
            {
                return parms.hostileFaction == Find.FactionManager.FirstFactionOfDef(def.faction);
            }
            return true;
        }
        return false;
    }

    protected override IEnumerable<PawnKindDef> GetPawnKindsForPoints(float points)
    {
        return PawnUtility.GetCombatPawnKindsForPoints((PawnKindDef k) => k.defName == "FCP_Pawnkind_Ghoul_Feral", points);
    }
}
