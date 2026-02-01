using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI.Group;
using Verse.Noise;

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
            Log.Message("Test running GeneratePawns");
            if (Find.FactionManager.GetFactions().Any(c => c == faction.GetValue(slate)))
            {
                Log.Message("faction exists");
                SetVars(slate);
                return true;
            }
            Log.Message("faction doesn't exist");
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
            Log.Message(pointsString);
            float.TryParse(pointsString, out points);
            
            //Map map = site.GetValue(slate).Map;
            PawnGroupKindDef pawnGroup = DefDatabase<PawnGroupKindDef>.GetNamed(pawnGroupKind.GetValue(slate), false);
            foreach(PawnGroupMaker maker in settlementFaction.def.pawnGroupMakers)
            {
                Log.Message(maker.kindDef.defName);
            }
            if(!settlementFaction.def.pawnGroupMakers.Any(c => c.kindDef.defName == pawnGroup.defName))
            {
                Log.Message("Faction does not contain the inputted pawnGroupKind");
                pawnGroup = PawnGroupKindDefOf.Combat;
            }
            Log.Message(points);
            List<Pawn> Pawns = PawnGroupMakerUtility.GeneratePawns(new PawnGroupMakerParms
            {
                groupKind = pawnGroup != null ? pawnGroup : PawnGroupKindDefOf.Peaceful,
                points = points,
                faction = settlementFaction
            }).ToList();
            Log.Message(Pawns.Count);
            slate.Set(storeAs.GetValue(slate), Pawns);
        }
    }
}

