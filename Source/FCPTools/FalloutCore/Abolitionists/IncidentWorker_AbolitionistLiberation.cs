namespace FCP.Core.Abolitionists;

public class IncidentWorker_AbolitionistLiberation : IncidentWorker_RaidEnemy
{
    protected override bool CanFireNowSub(IncidentParms parms)
    {
        if (!AbolitionistsUtility.ColonyOwnsSlaves())
        {
            return false;
        }

        parms.faction ??= AbolitionistsUtility.GetFaction();
        if (parms.faction == null)
        {
            return false;
        }

        return base.CanFireNowSub(parms);
    }

    protected override bool TryResolveRaidFaction(IncidentParms parms)
    {
        Faction faction = AbolitionistsUtility.GetFaction();
        if (faction == null)
        {
            return false;
        }

        parms.faction = faction;
        return true;
    }
}
