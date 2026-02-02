using Verse.AI;

namespace FCP.Core.PowerArmor;

public class JobDriver_SwapPowerArmor : JobDriver_PowerArmorStation
{
	protected override int GetDuration()
	{
		return StationComp.Props.swapDuration;
	}

	protected override void DoAction()
	{
		foreach (Apparel apparel in pawn.apparel.WornApparel.Where(StationComp.IsPowerArmorApparel).ToList())
		{
			if (!StationComp.StoreApparel(pawn, apparel))
			{
				Log.Error(pawn?.ToString() + " could not store " + apparel.ToStringSafe());
				EndJobWith(JobCondition.Errored);
				return;
			}
		}
		foreach (Apparel apparel in StationComp.HeldApparels)
		{
			if (!StationComp.EquipApparel(pawn, apparel))
			{
				Log.Error(pawn?.ToString() + " could not equip " + apparel.ToStringSafe());
				EndJobWith(JobCondition.Errored);
				return;
			}
		}
	}
}
