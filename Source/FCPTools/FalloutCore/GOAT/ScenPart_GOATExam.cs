using RimWorld;
using Verse;

namespace FCP.Core.GOAT;

public class ScenPart_GOATExam : ScenPart
{
    public override void PostMapGenerate(Map map)
    {
        base.PostMapGenerate(map);
        Queue<Pawn> pawns = new Queue<Pawn>(map.mapPawns.FreeColonists);
        LongEventHandler.ExecuteWhenFinished(() => RunNext(pawns));
    }

    public override string Summary(Scenario scen) => "FCP_ScenPart_GOATExam".Translate();

    public static void RunNext(Queue<Pawn> pawns)
    {
        if (pawns.Count == 0)
            return;

        Pawn pawn = pawns.Dequeue();
        Find.WindowStack.Add(new Dialog_GOATExam(pawn, pawns));
    }
}
