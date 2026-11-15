using RimWorld.QuestGen;

namespace FCP.Core.RadiantQuests;

public class QuestNode_GeneratePawns : QuestNode
{
    [NoTranslate]
    public SlateRef<string> storeAs;
    public SlateRef<Faction> faction;
    public SlateRef<string> pawnGroupKind;
    public SlateRef<string> pointsToUse;

    protected override bool TestRunInt(Slate slate)
    {
        if (Find.FactionManager.GetFactions().Any(c => c == faction.GetValue(slate)))
        {
            slate.Set(storeAs.GetValue(slate), new List<Pawn>());
            return true;
        }
        return false;
    }

    protected override void RunInt()
    {
        SetVars(QuestGen.slate);
    }

    private void SetVars(Slate slate)
    {
        Faction settlementFaction = faction.GetValue(slate);
        float points = slate.Get<int>("points", 100);
        pointsToUse.TryGetValue(slate, out string pointsString);
        float.TryParse(pointsString, out points);
            
        PawnGroupKindDef pawnGroup = DefDatabase<PawnGroupKindDef>.GetNamed(pawnGroupKind.GetValue(slate), false);
        if(pawnGroup == null || !settlementFaction.def.pawnGroupMakers.Any(c => c.kindDef.defName == pawnGroup.defName))
        {
            pawnGroup = PawnGroupKindDefOf.Combat;
        }
        List<Pawn> Pawns = PawnGroupMakerUtility.GeneratePawns(new PawnGroupMakerParms
        {
            groupKind = pawnGroup != null ? pawnGroup : PawnGroupKindDefOf.Peaceful,
            points = points,
            faction = settlementFaction
        }).ToList();
        foreach (Pawn pawn in Pawns)
            Find.WorldPawns.PassToWorld(pawn, RimWorld.Planet.PawnDiscardDecideMode.KeepForever);
        slate.Set(storeAs.GetValue(slate), Pawns);
    }
}