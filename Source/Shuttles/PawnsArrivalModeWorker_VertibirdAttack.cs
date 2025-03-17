using UnityEngine;
namespace FCP_Shuttles
{
	public class PawnsArrivalModeWorker_VertibirdAttack : PawnsArrivalModeWorker
	{
		public override void Arrive(List<Pawn> pawns, IncidentParms parms)
		{
			var extension = parms.faction.def.GetModExtension<FactionModExtension>();
			if (extension != null && parms.target is Map map)
			{
				List<IntVec3> previousLandingSpots = new List<IntVec3>();
				int maxPawnCount = extension.maxPawnCountInOneShuttle;
				if (maxPawnCount <= 0)
				{
					maxPawnCount = pawns.Count;
				}
				for (int i = 0; i < pawns.Count; i += maxPawnCount)
				{
					List<Pawn> currentPawns = pawns.GetRange(i, Mathf.Min(maxPawnCount, pawns.Count - i));
					Thing thing = ThingMaker.MakeThing(extension.transportShipDef.shipThing);
					thing.SetFaction(parms.faction);
					TransportShip transportShip = TransportShipMaker.MakeTransportShip(extension.transportShipDef, currentPawns, thing);
					IntVec3 landingSpot;
					int tries = 0;
					do
					{
						if (CellFinder.TryFindRandomReachableNearbyCell(parms.spawnCenter, map, 
						tries + extension.minDistanceBetweenShuttles, 
						TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false),
						(IntVec3 c) => !c.Roofed(map) && !c.Fogged(map), null, out landingSpot));
						tries++;
						if (tries > 1000)
						{
							Log.Error("FCP_Shuttles: Could not find a suitable landing spot after 1000 tries.");
							break;
						}
					} while (landingSpot.InBounds(map) && landingSpot.IsValid 
					&& previousLandingSpots.Any(x => x.DistanceTo(landingSpot) < extension.minDistanceBetweenShuttles));
					previousLandingSpots.Add(landingSpot);
					transportShip.ArriveAt(landingSpot, map.Parent);
					transportShip.AddJobs(ShipJobDefOf.Unload, ShipJobDefOf.FlyAway);
				}
			}
		}

		public override bool TryResolveRaidSpawnCenter(IncidentParms parms)
		{
			if (parms.faction != null)
			{
				var extension = parms.faction.def.GetModExtension<FactionModExtension>();
				if (extension != null && parms.target is Map map)
				{
					return extension.transportShipDef != null
					&& DropCellFinder.FindSafeLandingSpot(out parms.spawnCenter, parms.faction, map,
					size: extension.transportShipDef.shipThing.size);
				}
			}
			return false;
		}
	}
}