namespace FCP.Core
{
    public class IncidentWorker_CaravanArrivalTaxCollector : IncidentWorker_TraderCaravanArrival
    {
        protected override PawnGroupKindDef PawnGroupKindDef => 
            FCPDefOf.FCP_PawnGroupKind_TaxCollector;
        
        private ModExtension_FactionTaxCollectors Extension => 
            def.GetModExtension<ModExtension_FactionTaxCollectors>();
        
        protected override bool TryResolveParmsGeneral(IncidentParms parms)
        {
            Faction faction = Find.FactionManager
                .FirstFactionOfDef(Extension.factionDef);
            
            if (faction == null)
                return false;
            
            parms.faction = faction;
            parms.traderKind = Extension.traderKindDef;
            
            return base.TryResolveParmsGeneral(parms);
        }

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            return base.CanFireNowSub(parms) && 
                   Find.FactionManager.FirstFactionOfDef(Extension.factionDef) != null;
        }
        
        protected override bool FactionCanBeGroupSource(Faction f, Map map, bool desperate = false)
        {
            if (!base.FactionCanBeGroupSource(f, map, desperate))
                return false;

            return f.def == Extension.factionDef;
        }
        
        protected override float TraderKindCommonality(TraderKindDef traderKind, Map map, Faction faction)
        {
            return traderKind != Extension.traderKindDef 
                ? 0f 
                : traderKind.CalculatedCommonality;
        }
        
        protected override void SendLetter(IncidentParms parms, List<Pawn> pawns, TraderKindDef traderKind)
        {
            TaggedString letterLabel = def.letterLabel ?? "Extension was Null";
            TaggedString letterText = def.letterText ?? "Extension was Null";
            letterText += "\n\n" + "LetterCaravanArrivalCommonWarning".Translate();
            
            PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(
                pawns, ref letterLabel, ref letterText, 
                "LetterRelatedPawnsNeutralGroup"
                    .Translate(Faction.OfPlayer.def.pawnsPlural), informEvenIfSeenBefore: true);
            
            SendStandardLetter(letterLabel, letterText, LetterDefOf.PositiveEvent, parms, pawns[0]);
        }
    }
}