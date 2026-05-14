using RimWorld;
using System.Collections.Generic;
using Verse;

namespace FCP.Factions;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class FactionExtension_SpawnConfig : DefModExtension
{
	public List<BiomeDef> allowedBiomes;
	public bool clustered;
	public int clusterRadius = 30;
}
