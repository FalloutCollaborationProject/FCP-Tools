using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;

namespace FCP.Core.RadiantQuests;

public class QuestPart_SendRewardCaravan : QuestPart
{
    public string inSignal;
    public Faction faction;
    public PawnKindDef traderKind;
    public List<PawnKindDef> guardKinds;
    public ThingDef currency;
    public float rewardValue;
    public int guardsCount = 2;
    public string letterLabel;
    public string letterText;

    private bool sent;

    public override void Notify_QuestSignalReceived(Signal signal)
    {
        base.Notify_QuestSignalReceived(signal);
        if (sent || signal.tag != inSignal) return;

        Map homeMap = Find.Maps.FirstOrDefault(m => m.IsPlayerHome);
        if (homeMap == null) return;

        sent = true;
        SpawnCaravan(homeMap);
    }

    private void SpawnCaravan(Map map)
    {
        List<Pawn> pawns = GeneratePawns();
        Thing payment = GeneratePayment();
        
        IntVec3 spawnLoc = CellFinder.RandomClosewalkCellNear(MapGenerator.PlayerStartSpot, map, 8);
        foreach (Pawn pawn in pawns)
        {
            GenSpawn.Spawn(pawn, CellFinder.RandomClosewalkCellNear(spawnLoc, map, 3), map);
        }

        IntVec3 dropLoc = DropCellFinder.TradeDropSpot(map);
        GenPlace.TryPlaceThing(payment, dropLoc, map, ThingPlaceMode.Near);

        SendLetter(payment);
        ScheduleDeparture(map, pawns);
    }

    private List<Pawn> GeneratePawns()
    {
        List<Pawn> pawns = new List<Pawn>();

        if (traderKind != null)
            pawns.Add(PawnGenerator.GeneratePawn(traderKind, faction));

        for (int i = 0; i < guardsCount; i++)
        {
            PawnKindDef kind = guardKinds?.RandomElement() ?? faction.RandomPawnKind();
            pawns.Add(PawnGenerator.GeneratePawn(kind, faction));
        }

        return pawns;
    }

    private Thing GeneratePayment()
    {
        ThingDef currencyDef = currency ?? ThingDefOf.Silver;
        Thing money = ThingMaker.MakeThing(currencyDef);
        money.stackCount = (int)rewardValue;
        return money;
    }

    private void SendLetter(Thing reward)
    {
        string label = letterLabel ?? "Payment arrived";
        string text = letterText ?? "Payment caravan has arrived.";
        Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.PositiveEvent, new LookTargets(reward));
    }

    private void ScheduleDeparture(Map map, List<Pawn> pawns)
    {
        IntVec3 exit = CellFinder.RandomEdgeCell(map);
        int departureTick = Find.TickManager.TicksGame + 1200;

        foreach (Pawn pawn in pawns)
        {
            if (pawn.Spawned)
            {
                pawn.jobs?.StartJob(JobMaker.MakeJob(JobDefOf.Goto, exit), JobCondition.InterruptForced);
                pawn.mindState.exitMapAfterTick = departureTick;
            }
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref inSignal, "inSignal");
        Scribe_References.Look(ref faction, "faction");
        Scribe_Defs.Look(ref traderKind, "traderKind");
        Scribe_Collections.Look(ref guardKinds, "guardKinds", LookMode.Def);
        Scribe_Defs.Look(ref currency, "currency");
        Scribe_Values.Look(ref rewardValue, "rewardValue");
        Scribe_Values.Look(ref guardsCount, "guardsCount", 2);
        Scribe_Values.Look(ref letterLabel, "letterLabel");
        Scribe_Values.Look(ref letterText, "letterText");
        Scribe_Values.Look(ref sent, "sent");
    }
}
