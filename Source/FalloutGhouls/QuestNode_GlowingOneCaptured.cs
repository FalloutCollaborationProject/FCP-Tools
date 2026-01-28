using RimWorld;
using RimWorld.QuestGen;
using System.Linq;
using Verse;

namespace FCP_Ghoul
{
    public class QuestNode_GlowingOneCaptured : QuestNode
    {
        [NoTranslate]
        public SlateRef<string> inSignal;
        public QuestNode node;
        public QuestNode elseNode;

        public override bool TestRunInt(Slate slate) => true;

        public override void RunInt()
        {
            if (inSignal.GetValue(QuestGen.slate).NullOrEmpty()) return;

            QuestGen.quest.AddPart(new QuestPart_GlowingOneCaptured
            {
                inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(QuestGen.slate)),
                node = node,
                elseNode = elseNode
            });
        }
    }

    public class QuestPart_GlowingOneCaptured : QuestPart
    {
        public string inSignal;
        public QuestNode node;
        public QuestNode elseNode;

        public override void Notify_QuestSignalReceived(Signal signal)
        {
            if (signal.tag != inSignal) return;

            bool captured = Find.Maps.SelectMany(m => m.mapPawns.AllPawns)
                .Any(p => p.kindDef?.defName == "FCP_Pawnkind_Ghoul_GlowingOne" && 
                         (p.IsPrisonerOfColony || (p.Faction?.IsPlayer ?? false)));

            (captured && node != null ? node : !captured && elseNode != null ? elseNode : null)?.RunInt();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref inSignal, "inSignal");
        }
    }
}
