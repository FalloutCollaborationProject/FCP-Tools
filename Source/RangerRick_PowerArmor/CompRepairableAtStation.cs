namespace RangerRick_PowerArmor;

public class CompProperties_RepairableAtStation : CompProperties
{
	public List<ThingDefFloatClass> repairResourcesPerHP;
	public float repairTimeCostPerHP;

	public CompProperties_RepairableAtStation()
	{
		compClass = typeof(CompRepairableAtStation);
	}
}

public class CompRepairableAtStation : ThingComp
{
	public CompProperties_RepairableAtStation Props => (CompProperties_RepairableAtStation)props;
}
