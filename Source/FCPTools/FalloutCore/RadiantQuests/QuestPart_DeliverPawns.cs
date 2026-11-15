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

        int requiredCount = 0;
        int deliveredCount = 0;
        List<Thing> items = null;
        foreach (Pawn pawn in pawnsToDeliver)
        {
            if (pawn == null) continue;
            requiredCount++;
            if (caravan.ContainsPawn(pawn))
            {
                deliveredCount++;
            }
            else if (allowDead && pawn.Dead && pawn.Corpse != null)
            {
                items ??= CaravanInventoryUtility.AllInventoryItems(caravan);
                if (items.Contains(pawn.Corpse))
                    deliveredCount++;
            }
        }

        if (deliveredCount == requiredCount)
        {
            Complete(outSignalSuccess);
            foreach (Pawn pawn in pawnsToDeliver)
            {
                if (pawn == null) continue;
                if (caravan.ContainsPawn(pawn))
                {
                    caravan.RemovePawn(pawn);
                    Find.WorldPawns.PassToWorld(pawn);
                }
                else
                {
                    pawn.Corpse?.Destroy();
                }
            }
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

    public override bool QuestPartReserves(Pawn p)
    {
        return pawnsToDeliver != null && pawnsToDeliver.Contains(p);
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
