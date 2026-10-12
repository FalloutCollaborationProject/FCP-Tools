using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace FCP.Factions;

public class WorldObjectCompProperties_SettlementTraders : WorldObjectCompProperties
{
    public WorldObjectCompProperties_SettlementTraders()
    {
        compClass = typeof(WorldObjectComp_SettlementTraders);
    }
}

public class WorldObjectComp_SettlementTraders : WorldObjectComp
{
    private const int RestockIntervalTicks = 15 * GenDate.TicksPerDay;

    private List<SettlementTrader> traders;
    private List<SettlementTraderStock> stocks = new List<SettlementTraderStock>();
    private bool resolvedFromDef;

    private List<SettlementTrader> Traders
    {
        get
        {
            if (traders == null && !resolvedFromDef)
            {
                resolvedFromDef = true;
                if (parent is Settlement settlement)
                {
                    FactionExtension_SettlementControl ext = settlement.Faction?.def.GetModExtension<FactionExtension_SettlementControl>();
                    NamedSettlement named = ext?.namedSettlements?.Find(n => n.name == settlement.Name);
                    if (named != null && !named.traders.NullOrEmpty())
                        traders = named.traders;
                }
            }
            return traders;
        }
    }

    public bool HasTraders => !Traders.NullOrEmpty();

    public void SetTraders(List<SettlementTrader> traderList)
    {
        traders = traderList;
    }

    private SettlementTraderStock GetStock(Settlement settlement, TraderKindDef kind)
    {
        SettlementTraderStock stock = stocks.Find(s => s.TraderKind == kind);
        if (stock == null)
        {
            stock = new SettlementTraderStock(settlement, kind);
            stocks.Add(stock);
        }
        else if (Find.TickManager.TicksGame - stock.generatedTick >= RestockIntervalTicks)
        {
            stock.GenerateStock();
        }
        return stock;
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Collections.Look(ref traders, "traders", LookMode.Deep);
        Scribe_Collections.Look(ref stocks, "stocks", LookMode.Deep);

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            stocks ??= new List<SettlementTraderStock>();
            stocks.RemoveAll(s => s == null || s.TraderKind == null);
        }
    }

    public override IEnumerable<Gizmo> GetCaravanGizmos(Caravan caravan)
    {
        if (!HasTraders)
            yield break;

        Settlement settlement = parent as Settlement;
        if (settlement == null || settlement.Faction == null || settlement.Faction.HostileTo(Faction.OfPlayer))
            yield break;

        if (CaravanVisitUtility.SettlementVisitedNow(caravan) != settlement)
            yield break;

        Command_Action command = new Command_Action();
        command.defaultLabel = "Trade with settlement";
        command.defaultDesc = "Choose a trader to trade with.";
        command.icon = settlement.Faction.def.FactionIcon;
        command.action = delegate
        {
            List<FloatMenuOption> opts = new List<FloatMenuOption>();
            for (int i = 0; i < traders.Count; i++)
            {
                SettlementTrader trader = traders[i];
                FloatMenuOption opt = new FloatMenuOption(trader.traderName, delegate
                {
                    Pawn negotiator = BestCaravanPawnUtility.FindBestNegotiator(caravan);
                    Find.WindowStack.Add(new Dialog_Trade(negotiator, GetStock(settlement, trader.traderKind)));
                });
                opts.Add(opt);
            }
            Find.WindowStack.Add(new FloatMenu(opts));
        };
        yield return command;
    }
}
