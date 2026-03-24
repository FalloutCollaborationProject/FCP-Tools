using RimWorld.QuestGen;

namespace FCP.Core.RadiantQuests
{
    public class QuestNode_GeneratePawns : QuestNode
    {
        [NoTranslate]
        public SlateRef<string> storeAs;
        public SlateRef<Faction> faction;
        public SlateRef<string> pawnGroupKind;
        public SlateRef<string> pointsToUse;

        protected override bool TestRunInt(Slate slate)
        {
            FCPLog.Verbose("Test running GeneratePawns");
            if (Find.FactionManager.GetFactions().Any(c => c == faction.GetValue(slate)))
            {
                FCPLog.Verbose("faction exists");
                SetVars(slate);
                return true;
            }
            FCPLog.Verbose("faction doesn't exist");
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
            FCPLog.Verbose(pointsString);
            float.TryParse(pointsString, out points);
            
            //Map map = site.GetValue(slate).Map;
            PawnGroupKindDef pawnGroup = DefDatabase<PawnGroupKindDef>.GetNamed(pawnGroupKind.GetValue(slate), false);
            foreach(PawnGroupMaker maker in settlementFaction.def.pawnGroupMakers)
            {
                FCPLog.Verbose(maker.kindDef.defName);
            }
            if(!settlementFaction.def.pawnGroupMakers.Any(c => c.kindDef.defName == pawnGroup.defName))
            {
                FCPLog.Verbose("Faction does not contain the inputted pawnGroupKind");
                pawnGroup = PawnGroupKindDefOf.Combat;
            }
            FCPLog.Verbose(points);
            List<Pawn> Pawns = PawnGroupMakerUtility.GeneratePawns(new PawnGroupMakerParms
            {
                groupKind = pawnGroup != null ? pawnGroup : PawnGroupKindDefOf.Peaceful,
                points = points,
                faction = settlementFaction
            }).ToList();
            FCPLog.Verbose(Pawns.Count);
            slate.Set(storeAs.GetValue(slate), Pawns);
        }
    }
}

