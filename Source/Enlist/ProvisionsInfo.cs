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
		if (option.provisions != null)
		{
			foreach (ProvisionRecord provisionData in option.provisions)
			{
				Thing thing = ThingMaker.MakeThing(provisionData.thing, provisionData.stuff);
				thing.stackCount = provisionData.amountRange.RandomInRange;
				CaravanInventoryUtility.GiveThing(caravan, thing);
			}
		}
		if (option.recruits != null)
		{
			foreach (RecruitRecord recruit in option.recruits)
			{
				int num = recruit.count.RandomInRange;
				for (int i = 0; i < num; i++)
				{
					Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(recruit.pawnKind, Faction.OfPlayer, PawnGenerationContext.PlayerStarter, forceGenerateNewPawn: true));
					Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.KeepForever);
					caravan.AddPawn(pawn, false);
				}
			}
		}
		givenProvisionsLastTick = Find.TickManager.TicksGame;
	}
	public void ExposeData()
	{
		Scribe_Values.Look(ref givenProvisionsLastTick, "givenProvisionsLastTick");
	}
}