using RimWorld.QuestGen;

namespace FCP.Core.RadiantQuests;

public class QuestNode_GetPawnKind : QuestNode
{
    [NoTranslate]
    public SlateRef<string> storeAs;

    public SlateRef<string> pawnKindDef;

    protected override bool TestRunInt(Slate slate)
    {
        if (DefDatabase<PawnKindDef>.AllDefsListForReading.Any(c => c.defName == pawnKindDef.GetValue(slate)))
        {
            SetVars(slate);
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
        PawnKindDef def = DefDatabase<PawnKindDef>.AllDefsListForReading.First(c => c.defName == pawnKindDef.GetValue(slate));
        slate.Set(storeAs.GetValue(slate), def);
    }
}