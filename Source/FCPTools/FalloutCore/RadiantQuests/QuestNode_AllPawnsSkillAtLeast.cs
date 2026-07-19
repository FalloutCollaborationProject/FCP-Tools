using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace FCP.Core.RadiantQuests;

public class QuestNode_AllPawnsSkillAtLeast : QuestNode
{
    public SlateRef<IEnumerable<Pawn>> pawns;
    public SlateRef<SkillDef> skill;
    public SlateRef<int> level;
    public QuestNode node;

    protected override bool TestRunInt(Slate slate) => true;

    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        IEnumerable<Pawn> pawnList = pawns.GetValue(slate);
        SkillDef skillDef = skill.GetValue(slate);
        int threshold = level.GetValue(slate);
        if (pawnList == null || skillDef == null)
        {
            return;
        }

        bool allMeetThreshold = true;
        foreach (Pawn pawn in pawnList)
        {
            SkillRecord record = pawn?.skills?.GetSkill(skillDef);
            if (record == null || record.Level < threshold)
            {
                allMeetThreshold = false;
                break;
            }
        }

        if (allMeetThreshold)
        {
            node?.Run();
        }
    }
}
