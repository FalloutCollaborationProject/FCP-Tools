using RimWorld.QuestGen;

namespace FCP.Core.RadiantQuests;

public class QuestNode_SetPawnName : QuestNode
{
    public SlateRef<Pawn> pawn;
    [NoTranslate]
    public SlateRef<string> name;

    protected override bool TestRunInt(Slate slate)
    {
        RunInt();
        return true;
    }

    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        Pawn target = pawn.GetValue(slate);
        string pawnName = name.GetValue(slate);
        if (target == null || pawnName.NullOrEmpty())
        {
            return;
        }

        target.Name = new NameSingle(pawnName);
    }
}
