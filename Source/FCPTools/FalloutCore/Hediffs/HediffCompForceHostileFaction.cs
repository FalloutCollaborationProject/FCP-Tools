using RimWorld;
using Verse.AI.Group;

namespace FCP.Core.Hediffs;

public class HediffCompProperties_ForceHostileFaction : HediffCompProperties
{
    public FactionDef factionDef;

    public HediffCompProperties_ForceHostileFaction() =>
        compClass = typeof(HediffComp_ForceHostileFaction);
}

public class HediffComp_ForceHostileFaction : HediffComp
{
    public HediffCompProperties_ForceHostileFaction Props =>
        (HediffCompProperties_ForceHostileFaction)props;

    public override void CompPostPostAdd(DamageInfo? dinfo)
    {
        base.CompPostPostAdd(dinfo);
        EnsureHostile();
    }

    public override void Notify_Spawned()
    {
        base.Notify_Spawned();
        EnsureHostile();
    }

    private void EnsureHostile()
    {
        Faction faction = Find.FactionManager.FirstFactionOfDef(Props.factionDef) ?? Faction.OfPirates;

        if (Pawn.Faction != faction)
        {
            Pawn.SetFaction(faction);
        }

        if (!Pawn.Spawned || Pawn.Map == null || Pawn.GetLord() != null)
        {
            return;
        }

        LordJob_AssaultColony lordJob = new LordJob_AssaultColony(faction, canKidnap: false, canTimeoutOrFlee: false);
        LordMaker.MakeNewLord(faction, lordJob, Pawn.Map, Gen.YieldSingle(Pawn));
    }
}
