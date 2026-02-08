using FCP.Core;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace FCP.Enlist;

public class WorldEnlistTracker : WorldComponent
{
    public Dictionary<Faction, FactionOptions> factionOptionsContainer;
    public static WorldEnlistTracker Instance;
    public WorldEnlistTracker(World world) : base(world)
    {
        factionOptionsContainer = new Dictionary<Faction, FactionOptions>();
        Instance = this;
    }

    public IEnumerable<Faction> EnlistedFactions(FactionEnlistOptionsDef optionsDef)
    {
        if (FCPCoreMod.SettingsTab<EnlistSettings>().enlistStates[optionsDef.defName])
        {
            foreach (KeyValuePair<Faction, FactionOptions> data in factionOptionsContainer)
            {
                if (data.Value.factionsRecruiters.TryGetValue(optionsDef, out bool value) && value)
                {
                    yield return data.Key;
                }
            }
        }
    }

    public IEnumerable<Faction> EnlistedFactions()
    {
        foreach (KeyValuePair<Faction, FactionOptions> data in factionOptionsContainer)
        {
            foreach (KeyValuePair<FactionEnlistOptionsDef, bool> subData in data.Value.factionsRecruiters)
            {
                if (FCPCoreMod.SettingsTab<EnlistSettings>().enlistStates[subData.Key.defName] && subData.Value)
                {
                    yield return data.Key;
                }
            }
        }
    }
    public bool CanEnlist(Faction faction, FactionEnlistOptionsDef optionDef, out string cannotEnlistReason)
    {
        cannotEnlistReason = "";
        if (optionDef.Worker.CanEnlistTo(faction, out string cannotReason) is false)
        {
            cannotEnlistReason = cannotReason;
            return false;
        }
        if (optionDef.minGoodwillRequrementToEnlist.HasValue && faction.GoodwillWith(Faction.OfPlayer) < optionDef.minGoodwillRequrementToEnlist)
        {
            cannotEnlistReason = optionDef.enlistRequrementsNotSatisfiedKey.Translate(faction.GoodwillWith(Faction.OfPlayer));
            return false;
        }
        Faction firstHostileFaction = EnlistedFactions().FirstOrDefault(x => x.HostileTo(faction));
        if (firstHostileFaction != null)
        {
            cannotEnlistReason = optionDef.enlistedToHostileFactionKey.Translate(firstHostileFaction.Named("HOSTILEFACTION"));
            return false;
        }
        return true;
    }
    public bool EnlistedTo(Faction otherFaction, FactionEnlistOptionsDef options)
    {
        return otherFaction != null && options != null && factionOptionsContainer.TryGetValue(otherFaction, out FactionOptions factionOptions)
               && factionOptions != null && factionOptions.factionsRecruiters.TryGetValue(options, out bool enlistStatus) 
               && enlistStatus;
    }

    public void Delist(Faction otherFaction, FactionEnlistOptionsDef options)
    {
        factionOptionsContainer[otherFaction].factionsRecruiters[options] = false;
        if (options.resignGoodwillGain != 0)
        {
            otherFaction.TryAffectGoodwillWith(Faction.OfPlayer, options.resignGoodwillGain);
            Find.LetterStack.ReceiveLetter(options.resignLetterTitleKey.Translate(otherFaction.Named("FACTION")),
                options.resignLetterLabelKey.Translate(otherFaction.Named("FACTION"), otherFaction.leader.Named("LEADER")), LetterDefOf.NegativeEvent);
        }
    }

    public void KickOut(Faction otherFaction, FactionEnlistOptionsDef options)
    {
        factionOptionsContainer[otherFaction].factionsRecruiters[options] = false;
        Find.LetterStack.ReceiveLetter(options.kickOutLetterTitleKey.Translate(otherFaction.Named("FACTION")),
            options.kickOutLetterLabelKey.Translate(otherFaction.Named("FACTION"), otherFaction.leader.Named("LEADER")), LetterDefOf.NegativeEvent);
    }

    public bool Bought(Faction otherFaction, FactionEnlistOptionsDef options)
    {
        return otherFaction != null && options != null && factionOptionsContainer.TryGetValue(otherFaction, out FactionOptions factionOptions)
               && factionOptions != null && factionOptions.factionsBought.TryGetValue(options, out var boughtStatus)
               && boughtStatus != null;
    }


    public bool CanCallReinforcementFrom(Faction otherFaction, FactionEnlistOptionsDef options)
    {
        return !factionOptionsContainer[otherFaction].factionsReinforcementsLastTick.TryGetValue(options, out int value) || Find.TickManager.TicksGame >= value + options.reinforcementCallCooldownTicks;
    }

    public void CallReinforcement(Faction otherFaction, FactionEnlistOptionsDef options, Pawn caller, IntVec3 cell)
    {
        factionOptionsContainer[otherFaction].factionsReinforcementsLastTick[options] = Find.TickManager.TicksGame + options.reinforcementCallCooldownTicks;
        int curGoodwill = otherFaction.GoodwillWith(caller.Faction);
        float reinforcementPoints = GetReinforcementPoints(curGoodwill, options);
        IncidentParms parms = new IncidentParms
        {
            target = caller.Map,
            spawnCenter = cell,
            points = reinforcementPoints,
            faction = otherFaction
        };
        PawnsArrivalModeDef obj = PawnsArrivalModeDefOf.CenterDrop;
        obj.Worker.TryResolveRaidSpawnCenter(parms);
        PawnGroupMakerParms defaultPawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(PawnGroupKindDefOf.Combat, parms);
        defaultPawnGroupMakerParms.generateFightersOnly = true;
        List<Pawn> pawns = PawnGroupMakerUtility.GeneratePawns(defaultPawnGroupMakerParms).ToList();
        obj.Worker.Arrive(pawns, parms);
        LordMaker.MakeNewLord(otherFaction, new LordJob_AssistColony(parms.faction, cell), caller.Map, pawns);
        otherFaction.TryAffectGoodwillWith(Faction.OfPlayer, options.reinforcementCallGoodwillCost);
    }

    public float GetReinforcementPoints(int goodwill, FactionEnlistOptionsDef options)
    {
        foreach (ReinforcementOption option in options.reinforcementOptions)
        {
            if (option.relationsRange.Includes(goodwill))
            {
                return option.pointsRange.RandomInRange;
            }
        }
        return 0f;
    }

    public override void FinalizeInit(bool fromLoad)
    {
        base.FinalizeInit(fromLoad);
        Instance = this;
    }

    private bool rechecked;
    public override void WorldComponentTick()
    {
        base.WorldComponentTick();
        if (!rechecked)
        {
            RemoveCompsIfNeeded();
        }
    }

    private void RemoveCompsIfNeeded()
    {
        foreach (WorldObject worldObject in Find.WorldObjects.AllWorldObjects)
        {
            WorldObjectCompEnlist comp = worldObject.GetComponent<WorldObjectCompEnlist>();
            if (comp != null)
            {
                FactionEnlistOptions extension = comp.parent.Faction?.def?.GetModExtension<FactionEnlistOptions>();
                if (extension is null)
                {
                    worldObject.comps.Remove(comp);
                }
            }
        }
        rechecked = true;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref factionOptionsContainer, "factionOptionsContainer", LookMode.Reference, LookMode.Deep, ref factionKeys, ref factionOptionsValues);
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            factionOptionsContainer ??= new Dictionary<Faction, FactionOptions>();
        }
        Instance = this;
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            RemoveCompsIfNeeded();
        }
    }

    private List<Faction> factionKeys;
    private List<FactionOptions> factionOptionsValues;
}