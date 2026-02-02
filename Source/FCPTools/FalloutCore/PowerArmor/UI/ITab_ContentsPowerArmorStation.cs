namespace FCP.Core.PowerArmor;

public class ITab_ContentsPowerArmorStation : ITab_ContentsBase
{
	public override IList<Thing> container
	{
		get
		{
			var comp = SelThing?.TryGetComp<CompPowerArmorStation>();
			if (comp == null)
			{
				return new List<Thing>();
			}
			return comp.HeldApparels.Cast<Thing>().ToList();
		}
	}

	public ITab_ContentsPowerArmorStation()
	{
		labelKey = "TabCasketContents";
		containedItemsKey = "ContainedItems";
		canRemoveThings = false;
	}
}
