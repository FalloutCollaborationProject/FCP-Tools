using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace FCP.Core.Shuttles
{
	public class PawnsArrivalModeWorker_VertibirdAttack : PawnsArrivalModeWorker
	{
		public override void Arrive(List<Pawn> pawns, IncidentParms parms)
		{
			var extension = parms.faction.def.GetModExtension<FactionModExtension>();
			if (extension != null && parms.target is Map map)
			{
				ShuttleArrivalAction.Arrive(pawns.Cast<Thing>().ToList(), map, parms.faction, extension, parms.spawnCenter);
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
