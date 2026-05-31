using RimWorld.QuestGen;
using Verse;

namespace FCP.Core.RadiantQuests;

public class QuestNode_DownPawns : QuestNode
{
    public SlateRef<IEnumerable<Pawn>> pawns;

    protected override bool TestRunInt(Slate slate) => true;

    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        IEnumerable<Pawn> pawnList = pawns.GetValue(slate);
        if (pawnList == null) return;

        foreach (Pawn pawn in pawnList)
        {
            if (pawn.health?.hediffSet != null)
            {
                HealthUtility.DamageUntilDowned(pawn);
            }
        }
    }
}
