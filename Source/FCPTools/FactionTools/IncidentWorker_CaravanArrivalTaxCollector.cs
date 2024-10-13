using FCP.Core;

namespace FCP.Factions;

public class FactionTaxCollectorsExtension : DefModExtension
{
    public FactionDef factionDef;
    public TraderKindDef traderKindDef;
}

public class IncidentWorker_CaravanArrivalTaxCollector : IncidentWorker_TraderCaravanArrival
{
    protected override PawnGroupKindDef PawnGroupKindDef => FCPDefOf.FCP_PawnGroupKind_TaxCollector;
    private FactionTaxCollectorsExtension Extension => def.GetModExtension<FactionTaxCollectorsExtension>();

    protected override bool TryResolveParmsGeneral(IncidentParms parms)
    {
        var faction = Find.FactionManager.FirstFactionOfDef(Extension.factionDef);
        if (faction == null)
            return false;
        
        parms.faction = faction;
        parms.traderKind = Extension.traderKindDef;
        
        if (!base.TryResolveParmsGeneral(parms))
            return false;

        return true;
    }

    protected override bool CanFireNowSub(IncidentParms parms)
    {
        if (!base.CanFireNowSub(parms) || Find.FactionManager.FirstFactionOfDef(Extension.factionDef) == null)
            return false;
        
        return true;
    }

    protected override bool FactionCanBeGroupSource(Faction f, Map map, bool desperate = false)
    {
        if (!base.FactionCanBeGroupSource(f, map, desperate))
            return false;

        return f.def == Extension.factionDef;
    }

    protected override float TraderKindCommonality(TraderKindDef traderKind, Map map, Faction faction)
    {
        if (traderKind != Extension.traderKindDef)
            return 0f;
        
        return traderKind.CalculatedCommonality;
    }

    protected override void SendLetter(IncidentParms parms, List<Pawn> pawns, TraderKindDef traderKind)
    {
        TaggedString letterLabel = def.letterLabel ?? "Extension was Null";
        TaggedString letterText = def.letterText ?? "Extension was Null";
        letterText += "\n\n" + "LetterCaravanArrivalCommonWarning".Translate();
        
        PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(pawns, ref letterLabel, ref letterText, "LetterRelatedPawnsNeutralGroup".Translate(Faction.OfPlayer.def.pawnsPlural), informEvenIfSeenBefore: true);
        SendStandardLetter(letterLabel, letterText, LetterDefOf.PositiveEvent, parms, pawns[0]);
    }
}