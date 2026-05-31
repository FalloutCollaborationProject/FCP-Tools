using RimWorld;
using RimWorld.Planet;
using Verse;

namespace FCP.Core.RadiantQuests;

public class QuestPart_DeliverPawns : QuestPart
{
    public string inSignal;
    public List<Pawn> pawnsToDeliver;
    public string outSignalSuccess;
    public string outSignalFailed;
    public bool allowDead;

    public override void Notify_QuestSignalReceived(Signal signal)
    {
        base.Notify_QuestSignalReceived(signal);
        
        if (signal.tag != inSignal) return;

        if (!(signal.args.TryGetArg("CARAVAN", out Caravan caravan)))
        {
            Complete(outSignalFailed);
            return;
        }

        int deliveredCount = 0;
        foreach (Pawn pawn in pawnsToDeliver)
        {
            if (pawn == null) continue;
            if (!allowDead && pawn.Dead) continue;
            if (caravan.ContainsPawn(pawn))
            {
                deliveredCount++;
            }
        }

        if (deliveredCount == pawnsToDeliver.Count)
        {
            Complete(outSignalSuccess);
        }
        else
        {
            Complete(outSignalFailed);
        }
    }

    private void Complete(string signal)
    {
        if (!signal.NullOrEmpty())
        {
            Find.SignalManager.SendSignal(new Signal(signal, quest.Named("SUBJECT")));
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref inSignal, "inSignal");
        Scribe_Values.Look(ref outSignalSuccess, "outSignalSuccess");
        Scribe_Values.Look(ref outSignalFailed, "outSignalFailed");
        Scribe_Values.Look(ref allowDead, "allowDead");
        Scribe_Collections.Look(ref pawnsToDeliver, "pawnsToDeliver", LookMode.Reference);
    }
}
