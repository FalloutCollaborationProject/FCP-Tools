namespace RangerRick_PowerArmor;

public class JobDriver_StorePowerArmor : JobDriver_PowerArmorStation
{
	protected override int GetDuration()
	{
		return StationComp.Props.storeDuration;
	}

	protected override int GetUnequipDuration()
	{
		return 0;
	}

	protected override void TryUnequipSomething()
	{
	}

	protected override void DoAction()
	{
		foreach (Apparel apparel in pawn.apparel.WornApparel.Where(StationComp.IsPowerArmorApparel).ToList())
		{
			if (!StationComp.StoreApparel(pawn, apparel))
			{
				pawn.apparel.Wear(apparel);
			}
		}
	}
}
