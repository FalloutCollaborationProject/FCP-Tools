using RimWorld.Planet;

namespace FCP.Core.WastelandEncounters;

public class IncidentWorker_CaravanWastelandCache : IncidentWorker
{
    protected override bool CanFireNowSub(IncidentParms parms)
    {
        return parms.target is Caravan caravan && caravan.PawnsListForReading.Count > 0 && base.CanFireNowSub(parms);
    }

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        if (parms.target is not Caravan caravan)
        {
            return false;
        }

        Pawn carrier = caravan.PawnsListForReading.RandomElement();
        if (carrier?.inventory == null)
        {
            return false;
        }

        ThingDef capsDef = DefDatabase<ThingDef>.GetNamedSilentFail("FCP_Currency_Caps");
        if (capsDef == null)
        {
            return false;
        }

        Thing caps = ThingMaker.MakeThing(capsDef);
        caps.stackCount = Rand.RangeInclusive(30, 90);
        carrier.inventory.innerContainer.TryAdd(caps, true);

        SendStandardLetter(parms, new LookTargets(caravan));
        return true;
    }
}
