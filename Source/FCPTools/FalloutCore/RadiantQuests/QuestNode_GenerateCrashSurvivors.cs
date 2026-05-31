using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace FCP.Core.RadiantQuests;

public class QuestNode_GenerateCrashSurvivors : QuestNode
{
    public SlateRef<Faction> faction;
    public SlateRef<string> storeAs;
    public SlateRef<PawnKindDef> pawnKind;

    protected override bool TestRunInt(Slate slate)
    {
        return true;
    }

    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        Faction fac = faction != null ? faction.GetValue(slate) : null;
        if (fac == null && slate.Exists("askerFaction"))
        {
            fac = slate.Get<Faction>("askerFaction");
        }
        if (fac == null) return;
        
        int tile = -1;
        if (slate.Exists("site"))
        {
            MapParent site = slate.Get<MapParent>("site");
            if (site != null) tile = site.Tile;
        }
        
        PawnKindDef kind = null;
        if (pawnKind != null && pawnKind.TryGetValue(slate, out PawnKindDef pk))
        {
            kind = pk;
        }
        else if (fac.def.basicMemberKind != null)
        {
            kind = fac.def.basicMemberKind;
        }
        else if (fac.def.pawnGroupMakers != null)
        {
            foreach (var maker in fac.def.pawnGroupMakers)
            {
                if (maker.options != null && maker.options.Count > 0)
                {
                    kind = maker.options.First().kind;
                    break;
                }
            }
        }
        
        if (kind == null) return;
        
        PawnGenerationRequest request = new PawnGenerationRequest(kind, fac, PawnGenerationContext.NonPlayer, tile, forceGenerateNewPawn: true, allowDead: false, allowDowned: false, canGeneratePawnRelations: false);
        
        List<Pawn> pawns = new List<Pawn>(8);
        for (int i = 0; i < 8; i++)
        {
            pawns.Add(PawnGenerator.GeneratePawn(request));
        }

        slate.Set(storeAs.GetValue(slate), pawns);
    }
}
