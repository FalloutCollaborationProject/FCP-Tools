using UnityEngine;

namespace RangerRick_PowerArmor;

public class CompProperties_PowerArmorStation : CompProperties
{
	public Vector3 armorDrawOffset;

	public List<ApparelLayerDef> allowedLayers;

	public int storeDuration = 120;

	public int equipDuration = 120;

	public int swapDuration = 240;

	public CompProperties_PowerArmorStation()
	{
		compClass = typeof(CompPowerArmorStation);
	}
}
