using FCP.Enlist;

namespace FCP.Synths;

public class PlaceWorker_RequiresInstituteEnlistment : PlaceWorker
{
    public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
    {
        Faction institute = Find.FactionManager.FirstFactionOfDef(SynthDefOf.FCP_Faction_Institute);
        if (institute == null)
            return "FCP_RelayTerminal_NoInstitute".Translate();

        if (!WorldEnlistTracker.Instance.EnlistedTo(institute, SynthDefOf.FCP_Enlistment_Institute))
            return "FCP_RelayTerminal_RequiresEnlistment".Translate();

        return true;
    }
}
