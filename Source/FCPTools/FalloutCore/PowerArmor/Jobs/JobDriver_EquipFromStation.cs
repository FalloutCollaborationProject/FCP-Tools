using Verse.AI;

namespace FCP.Core.PowerArmor;

public class JobDriver_EquipFromStation : JobDriver_PowerArmorStation
{
	protected override int GetDuration()
	{
		return StationComp.Props.equipDuration;
	}

	protected override void DoAction()
	{
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
