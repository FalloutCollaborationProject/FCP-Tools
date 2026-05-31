using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace FCP.Factions;

public class WorldObjectComp_SettlementTraders : WorldObjectComp
{
	private List<SettlementTrader> traders;

	public void SetTraders(List<SettlementTrader> traderList)
	{
		traders = traderList;
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Collections.Look(ref traders, "traders", LookMode.Deep);
	}

	public override IEnumerable<Gizmo> GetCaravanGizmos(Caravan caravan)
	{
		if (traders == null || traders.Count == 0)
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
					SettlementTraderStock stock = new SettlementTraderStock(settlement, trader.traderKind);
					Find.WindowStack.Add(new Dialog_Trade(negotiator, stock));
				});
				opts.Add(opt);
			}
			Find.WindowStack.Add(new FloatMenu(opts));
		};
		yield return command;
	}
}
