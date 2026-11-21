using RimWorld.QuestGen;

namespace FCP.Core.RadiantQuests;

public class QuestNode_GetPawnKind : QuestNode
{
    [NoTranslate]
    public SlateRef<string> storeAs;

    public SlateRef<string> pawnKindDef;

    protected override bool TestRunInt(Slate slate)
    {
        if (DefDatabase<PawnKindDef>.GetNamedSilentFail(pawnKindDef.GetValue(slate)) != null)
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
        PawnKindDef def = DefDatabase<PawnKindDef>.GetNamed(pawnKindDef.GetValue(slate));
        slate.Set(storeAs.GetValue(slate), def);
    }
}