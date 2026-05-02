using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace FCP.Enlist;

public class DeliveryQuestList : IExposable
{
	public List<DeliveryQuest> quests = new List<DeliveryQuest>();
	public int lastRefreshTick;

	public void ExposeData()
	{
		Scribe_Collections.Look(ref quests, "quests", LookMode.Deep);
		Scribe_Values.Look(ref lastRefreshTick, "lastRefreshTick");
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
			quests ??= new List<DeliveryQuest>();
	}
}

public class DeliveryQuest : IExposable
{
	public ThingDef thingToDeliver;
	public ThingDef stuff;
	public int count;
	public int reward;
	public ThingDef rewardDef;
	public int sourceTile;
	public int destinationTile;
	public int createdTick;
	public int durationTicks;
	public bool accepted;

	public bool IsExpired => !accepted && Find.TickManager.TicksGame > createdTick + durationTicks;

	public bool CaravanCanTurnIn(Caravan caravan)
	{
		return caravan.AllThings
			.Where(t => t.def == thingToDeliver && (stuff == null || t.Stuff == stuff))
			.Sum(t => t.stackCount) >= count;
	}

	public void TurnIn(Caravan caravan, FactionEnlistOptionsDef optionDef)
	{
		int remaining = count;
		foreach (Thing item in caravan.AllThings.Where(t => t.def == thingToDeliver && (stuff == null || t.Stuff == stuff)).ToList())
		{
			int take = Mathf.Min(remaining, item.stackCount);
			item.SplitOff(take)?.Destroy();
			remaining -= take;
			if (remaining <= 0) break;
		}
		Thing payment = ThingMaker.MakeThing(rewardDef ?? optionDef.salaryDef ?? ThingDefOf.Silver);
		payment.stackCount = reward;
		CaravanInventoryUtility.GiveThing(caravan, payment);
	}

	public void ExposeData()
	{
		Scribe_Defs.Look(ref thingToDeliver, "thingToDeliver");
		Scribe_Defs.Look(ref stuff, "stuff");
		Scribe_Values.Look(ref count, "count");
		Scribe_Values.Look(ref reward, "reward");
		Scribe_Defs.Look(ref rewardDef, "rewardDef");
		Scribe_Values.Look(ref sourceTile, "sourceTile");
		Scribe_Values.Look(ref destinationTile, "destinationTile");
		Scribe_Values.Look(ref createdTick, "createdTick");
		Scribe_Values.Look(ref durationTicks, "durationTicks");
		Scribe_Values.Look(ref accepted, "accepted");
	}
}
