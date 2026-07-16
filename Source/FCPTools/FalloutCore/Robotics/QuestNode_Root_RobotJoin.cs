using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace FCP.Core.Robotics
{
    public class QuestNode_Root_RobotJoin : QuestNode_Root_WandererJoin
    {
        private const int TimeoutTicks = 30000;

        public SlateRef<PawnKindDef> kindDef;

        private string signalAccept;
        private string signalReject;

        protected override void RunInt()
        {
            base.RunInt();
            Quest quest = QuestGen.quest;
            quest.Delay(TimeoutTicks, delegate
            {
                QuestGen_End.End(quest, QuestEndOutcome.Fail);
            });
        }

        public override Pawn GeneratePawn()
        {
            PawnKindDef kind = kindDef.GetValue(QuestGen.slate);
            Pawn pawn;
            WildRobotSpawn_Patch.Suppress = true;
            try
            {
                pawn = PawnGenerator.GeneratePawn(kind);
            }
            finally
            {
                WildRobotSpawn_Patch.Suppress = false;
            }

            if (!pawn.IsWorldPawn())
            {
                Find.WorldPawns.PassToWorld(pawn);
            }
            return pawn;
        }

        protected override void AddSpawnPawnQuestParts(Quest quest, Map map, Pawn pawn)
        {
            signalAccept = QuestGenUtility.HardcodedSignalWithQuestID("Accept");
            signalReject = QuestGenUtility.HardcodedSignalWithQuestID("Reject");

            quest.Signal(signalAccept, delegate
            {
                quest.SetFaction(Gen.YieldSingle(pawn), Faction.OfPlayer);
                quest.PawnsArrive(Gen.YieldSingle(pawn), null, map.Parent);
                QuestGen_End.End(quest, QuestEndOutcome.Success);
            });

            quest.Signal(signalReject, delegate
            {
                QuestGen_End.End(quest, QuestEndOutcome.Fail);
            });
        }

        [System.Obsolete]
        public override void SendLetter(Quest quest, Pawn pawn)
        {
            TaggedString letterTitle = "FCP_LetterLabel_RobotJoins".Translate(pawn.Named("PAWN")).AdjustedFor(pawn);
            TaggedString letterText = "FCP_Letter_RobotJoins".Translate(pawn.Named("PAWN")).AdjustedFor(pawn);
            ChoiceLetter_AcceptJoinerScenario choiceLetter = (ChoiceLetter_AcceptJoinerScenario)LetterMaker.MakeLetter(letterTitle, letterText, FCPDefOf.FCP_Letter_AcceptRobotJoin);
            choiceLetter.signalAccept = signalAccept;
            choiceLetter.signalReject = signalReject;
            choiceLetter.quest = quest;
            choiceLetter.overrideMap = Find.AnyPlayerHomeMap;
            choiceLetter.StartTimeout(TimeoutTicks);
            Find.LetterStack.ReceiveLetter(choiceLetter);
        }
    }
}
