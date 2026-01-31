using RimWorld;
using RimWorld.Planet;
using Verse;

namespace FCP.Enlist;

public class ProvisionsInfo : IExposable
{

	private int givenProvisionsLastTick;
	public bool CanGiveProvisions(ProvisionOption option)
	{
		return givenProvisionsLastTick == 0 || Find.TickManager.TicksGame >= givenProvisionsLastTick + (option.provisionsRestockInDays * GenDate.TicksPerDay);
	}
	public void GiveProvisions(ProvisionOption option, Caravan caravan)
	{
		foreach (ProvisionRecord provisionData in option.provisions)
		{
			Thing thing = ThingMaker.MakeThing(provisionData.thing, provisionData.stuff);
			thing.stackCount = provisionData.amountRange.RandomInRange;
			CaravanInventoryUtility.GiveThing(caravan, thing);
		}
		givenProvisionsLastTick = Find.TickManager.TicksGame;
	}
	public void ExposeData()
	{
		Scribe_Values.Look(ref givenProvisionsLastTick, "givenProvisionsLastTick");
	}
}