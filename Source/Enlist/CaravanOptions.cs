using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace FCP.Enlist;

public class CaravanOptions : IExposable
{
	public WorkOption curWorkOption;
	public int curEnlistOptionInd;
	public int curWorkOptionInd;
	public WorldObject curWorldObject;
	public List<FactionEnlistOptionsDef> OptionsDefs => curWorldObject.GetComponent<WorldObjectCompEnlist>().OptionsDefs;

	public CaravanOptions()
	{

	}
	public CaravanOptions(WorldObject worldObject)
	{
		curWorldObject = worldObject;
		Reset();
	}
	public void Reset()
	{
		curWorkOption = null;
		curWorkOptionInd = -1;
		curEnlistOptionInd = -1;
	}
	public void ExposeData()
	{
		Scribe_Values.Look(ref curEnlistOptionInd, "curEnlistOptionInd", -1);
		Scribe_Values.Look(ref curWorkOptionInd, "curWorkOptionInd", -1);
		Scribe_References.Look(ref curWorldObject, "curSettlement");
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			if (curEnlistOptionInd != -1 && curWorkOptionInd != -1 && OptionsDefs != null)
			{
				curWorkOption = OptionsDefs[curEnlistOptionInd].workOptions[curWorkOptionInd];
			}
		}
	}
}