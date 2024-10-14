// ReSharper disable UnassignedField.Global

namespace FCP.Core
{
    [UsedImplicitly]
    public class PawnFactionDefinition : PawnGenerationDefinition
    {
        public FactionDef factionDef;
        public RoyalTitleDef title;
        public bool useFactionalIdeo;

        public override bool AppliesPreGeneration => true;
        public override bool AppliesPostGeneration => title != null;

        public override void ApplyToRequest(ref PawnGenerationRequest request)
        {
            if (factionDef == null) return;
            Faction faction = Find.FactionManager.FirstFactionOfDef(factionDef);

            if (faction != null)
            {
                request.Faction = faction;
                request.FixedTitle = title ?? request.FixedTitle;
                request.FixedIdeo = useFactionalIdeo ? faction.ideos.PrimaryIdeo : request.FixedIdeo;
            }
            else
            {
                FCPLog.Warning("[PawnFactionDefinition] had a defined factionDef, but no faction exists with that def");
            }
        }

        public override void ApplyToPawn(Pawn pawn)
        {
            pawn.royalty?.AllFactionPermits?.Clear();
        }
    }
}