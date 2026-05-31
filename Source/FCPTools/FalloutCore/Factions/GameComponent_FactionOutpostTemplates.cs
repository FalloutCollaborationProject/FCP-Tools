using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace FCP.Factions;

public class GameComponent_FactionOutpostTemplates : GameComponent
{
	private Dictionary<int, NamedSettlement> templates = new Dictionary<int, NamedSettlement>();

	public GameComponent_FactionOutpostTemplates(Game game)
	{
	}

	public void RegisterSite(int tile, NamedSettlement template)
	{
		templates[tile] = template;
	}

	public NamedSettlement GetTemplate(int tile)
	{
		templates.TryGetValue(tile, out NamedSettlement result);
		return result;
	}

	public void RemoveTemplate(int tile)
	{
		templates.Remove(tile);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Collections.Look(ref templates, "templates", LookMode.Value, LookMode.Deep);
		if (Scribe.mode == LoadSaveMode.PostLoadInit && templates == null)
			templates = new Dictionary<int, NamedSettlement>();
	}
}
