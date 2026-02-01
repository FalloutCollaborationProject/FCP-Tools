using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace FCP.Core.Shuttles
{
	[HarmonyPatch(typeof(RoyalTitlePermitWorker_DropResources), "CallResources")]
	public static class RoyalTitlePermitWorker_DropResources_Patch
	{
		public static bool Prefix(RoyalTitlePermitWorker_DropResources __instance, IntVec3 cell)
		{
			// Use Traverse to access protected fields
			var faction = Traverse.Create(__instance).Property<Faction>("faction").Value;
			var map = Traverse.Create(__instance).Property<Map>("map").Value;
			var caller = Traverse.Create(__instance).Property<Pawn>("caller").Value;
			var free = Traverse.Create(__instance).Property<bool>("free").Value;
			
			var extension = faction.def.GetModExtension<FactionModExtension>();
			if (extension != null && extension.transportShipDef != null)
			{
				List<Thing> list = new List<Thing>();
				for (int i = 0; i < __instance.def.royalAid.itemsToDrop.Count; i++)
				{
					Thing thing = ThingMaker.MakeThing(__instance.def.royalAid.itemsToDrop[i].thingDef);
					thing.stackCount = __instance.def.royalAid.itemsToDrop[i].count;
					list.Add(thing);
				}
				if (list.Any())
				{
					Thing thing = ThingMaker.MakeThing(extension.transportShipDef.shipThing);
					thing.SetFaction(faction);
					TransportShip transportShip = TransportShipMaker.MakeTransportShip(extension.transportShipDef, list, thing);
					transportShip.ArriveAt(cell, map.Parent);
					transportShip.AddJobs(ShipJobDefOf.Unload, ShipJobDefOf.FlyAway);
					Messages.Message("MessagePermitTransportDrop".Translate(faction.Named("FACTION")), new LookTargets(cell, map), MessageTypeDefOf.NeutralEvent);
					caller.royalty.GetPermit(__instance.def, faction).Notify_Used();
					if (!free)
					{
						caller.royalty.TryRemoveFavor(faction, __instance.def.royalAid.favorCost);
					}
				}
				return false;
			}
			return true;
		}
	}
}
