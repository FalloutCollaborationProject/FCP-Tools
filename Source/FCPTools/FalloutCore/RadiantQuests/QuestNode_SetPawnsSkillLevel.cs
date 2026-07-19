using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace FCP.Core.RadiantQuests;

public class QuestNode_SetPawnsSkillLevel : QuestNode
{
    public SlateRef<IEnumerable<Pawn>> pawns;
    public SlateRef<SkillDef> skill;
    public SlateRef<int> level;

    protected override bool TestRunInt(Slate slate)
    {
        RunInt();
        return true;
    }

    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        IEnumerable<Pawn> pawnList = pawns.GetValue(slate);
        SkillDef skillDef = skill.GetValue(slate);
        int newLevel = level.GetValue(slate);
        if (pawnList == null || skillDef == null)
        {
            return;
        }

        foreach (Pawn pawn in pawnList)
        {
            SkillRecord record = pawn?.skills?.GetSkill(skillDef);
            if (record == null)
            {
                continue;
            }

            record.Level = newLevel;
            record.xpSinceLastLevel = 0f;
            record.passion = Passion.None;
        }
    }
}
