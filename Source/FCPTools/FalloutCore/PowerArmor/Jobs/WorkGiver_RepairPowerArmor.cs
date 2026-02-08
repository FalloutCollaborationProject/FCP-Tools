using System.Collections.Generic;
using System.Linq;
using FCP.Core.Access;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace FCP.Core.PowerArmor;

public class WorkGiver_RepairPowerArmor : WorkGiver_Scanner
{
	private List<ThingCount> chosenResources = new List<ThingCount>();

	public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial);

	public override PathEndMode PathEndMode => PathEndMode.Touch;

	public override Danger MaxPathDanger(Pawn pawn)
	{
		return Danger.Deadly;
	}

	private List<ThingDefFloatClass> GetRepairResources(CompPowerArmorStation stationComp)
	{
		List<ThingDefFloatClass> repairResources = new List<ThingDefFloatClass>();

		foreach (Apparel apparel in stationComp.HeldApparels)
		{
			if (apparel.HitPoints < apparel.MaxHitPoints)
			{
				var repairComp = apparel.GetComp<CompRepairableAtStation>();
				if (repairComp != null && repairComp.Props.repairResourcesPerHP != null && repairComp.Props.repairResourcesPerHP.Count > 0)
				{
					repairResources.AddRange(repairComp.Props.repairResourcesPerHP);
					break;
				}
			}
		}

		return repairResources;
	}

	private bool TryFindRepairResources(List<ThingDefFloatClass> repairResources, Pawn pawn, Thing building, List<ThingCount> chosen)
	{
		List<IngredientCount> ingredients = repairResources.Select((ThingDefFloatClass tc) => tc.ToIngredientCount()).ToList();

		IntVec3 rootCell = building is Building b && b.def.hasInteractionCell ? b.InteractionCell : building.Position;
		return AccessExtensions_WorkGiver_DoBill.P_TryFindBestIngredientsHelper(delegate (Thing t)
		{
			foreach (IngredientCount ingredient in ingredients)
			{
				if (ingredient.filter.Allows(t))
				{
					return true;
				}
			}
			return false;
		}, (List<Thing> foundThings) => AccessExtensions_WorkGiver_DoBill.P_TryFindBestIngredientsInSet_NoMixHelper(foundThings, ingredients, chosen, rootCell, alreadySorted: false, null), ingredients, pawn, building, chosen, 999f);
	}

	public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (!(t is Building building))
		{
			return false;
		}

		var stationComp = building.GetComp<CompPowerArmorStation>();
		if (stationComp == null)
		{
			return false;
		}

		if (!stationComp.HeldApparels.Any())
		{
			return false;
		}

		List<ThingDefFloatClass> repairResources = GetRepairResources(stationComp);
		if (repairResources.Count == 0)
		{
			return false;
		}

		if (!pawn.CanReserve(building, 1, -1, null, forced))
		{
			return false;
		}

		chosenResources.Clear();

		if (!TryFindRepairResources(repairResources, pawn, building, chosenResources))
		{
			Dictionary<ThingDef, int> foundCounts = new Dictionary<ThingDef, int>();
			foreach (ThingCount chosen in chosenResources)
			{
				foundCounts.TryAdd(chosen.Thing.def, 0);
				foundCounts[chosen.Thing.def] += chosen.Count;
			}

			List<string> missingParts = new List<string>();
			foreach (ThingDefFloatClass repairResource in repairResources)
			{
				int needed = Mathf.CeilToInt(repairResource.count);
				int found = foundCounts.ContainsKey(repairResource.thingDef) ? foundCounts[repairResource.thingDef] : 0;
				int missing = needed - found;
				if (missing > 0)
				{
					missingParts.Add($"{missing}x {repairResource.thingDef.label}");
				}
			}

			if (missingParts.Count > 0)
			{
				JobFailReason.Is("MissingMaterials".Translate(missingParts.ToCommaList()));
			}
			return false;
		}

		return true;
	}

	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		Building building = (Building)t;
		var stationComp = building.GetComp<CompPowerArmorStation>();

		List<ThingDefFloatClass> repairResources = GetRepairResources(stationComp);
		if (repairResources.Count == 0)
		{
			return null;
		}

		chosenResources.Clear();

		if (!TryFindRepairResources(repairResources, pawn, building, chosenResources))
		{
			return null;
		}

		Job job = JobMaker.MakeJob(PowerArmorDefOf.RR_RepairPowerArmor, building);
		job.targetQueueB = new List<LocalTargetInfo>(chosenResources.Count);
		job.countQueue = new List<int>(chosenResources.Count);

		foreach (ThingCount chosenResource in chosenResources)
		{
			job.targetQueueB.Add(chosenResource.Thing);
			job.countQueue.Add(chosenResource.Count);
		}

		job.haulMode = HaulMode.ToCellNonStorage;
		return job;
	}
}
