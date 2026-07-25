using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace FCP.Core.RadiantQuests;

public class QuestNode_WatchPawnsSkillAtLeast : QuestNode
{
    public SlateRef<IEnumerable<Pawn>> pawns;
    public SlateRef<SkillDef> skill;
    public SlateRef<int> level;
    public SlateRef<string> outSignalComplete;

    protected override bool TestRunInt(Slate slate) => true;

    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        QuestPart_WatchPawnsSkillAtLeast part = new QuestPart_WatchPawnsSkillAtLeast
        {
            pawns = pawns.GetValue(slate)?.ToList() ?? new List<Pawn>(),
            skill = skill.GetValue(slate),
            level = level.GetValue(slate),
            outSignalComplete = QuestGenUtility.HardcodedSignalWithQuestID(outSignalComplete.GetValue(slate))
        };
        QuestGen.quest.AddPart(part);
        part.StartWatching();
    }
}
