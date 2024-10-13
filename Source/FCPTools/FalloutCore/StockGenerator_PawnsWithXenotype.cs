namespace FCP.Core;

public class StockGenerator_PawnsWithXenotype : StockGenerator_Slaves
{
	private bool respectPopulationIntent = true;
	public bool ignoreIdeoRequirements = false;
	public XenotypeDef xenotypeDef = XenotypeDefOf.Baseliner;
	
	public override IEnumerable<Thing> GenerateThings(int forTile, Faction faction = null)
	{
		if (respectPopulationIntent && Rand.Value > StorytellerUtilityPopulation.PopulationIntent)
		{
			yield break;
		}
		
		// If not told to ignore, break if the ideo doesn't support slavery.
		if (!ignoreIdeoRequirements && faction?.ideos != null)
		{
			if (faction.ideos.AllIdeos.Any(ideo => !ideo.IdeoApprovesOfSlavery()))
			{
				Log.Warning($"Faction {faction.def.defName} has a StockGenerator_SlavesWithXenotype without ignoreIdeoRequirements but has an ideo that disapproves of slavery.");
				yield break;
			}
		}
		
		int generateCount = countRange.RandomInRange;
		for (int i = 0; i < generateCount; i++)
		{
			var pawnRequest = new PawnGenerationRequest(slaveKindDef ?? PawnKindDefOf.Slave, faction, PawnGenerationContext.NonPlayer, forTile, 
				forceGenerateNewPawn: false, 
				allowDead: false, 
				allowDowned: false, 
				canGeneratePawnRelations: true, 
				mustBeCapableOfViolence: false, 
				colonistRelationChanceFactor: 1f,
				forceAddFreeWarmLayerIfNeeded: !trader.orbital,
				allowGay: true,
				allowPregnant: false,
				allowFood: true,
				allowAddictions: true,
				inhabitant: false,
				certainlyBeenInCryptosleep: false,
				forceRedressWorldPawnIfFormerColonist: false,
				worldPawnFactionDoesntMatter: false);

			pawnRequest.ForcedXenotype = xenotypeDef;
			
			yield return PawnGenerator.GeneratePawn(pawnRequest);
		}
	}

}