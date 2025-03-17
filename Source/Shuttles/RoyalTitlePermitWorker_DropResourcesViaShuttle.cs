using HarmonyLib;

namespace FCP_Shuttles
{
	[HarmonyPatch(typeof(RoyalTitlePermitWorker_DropResources), "CallResources")]
	public static class RoyalTitlePermitWorker_DropResources_Patch
	{
		public static bool Prefix(RoyalTitlePermitWorker_DropResources __instance, IntVec3 cell)
		{
			var extension = __instance.faction.def.GetModExtension<FactionModExtension>();
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
					thing.SetFaction(__instance.faction);
					TransportShip transportShip = TransportShipMaker.MakeTransportShip(extension.transportShipDef, list, thing);
					transportShip.ArriveAt(cell, __instance.map.Parent);
					transportShip.AddJobs(ShipJobDefOf.Unload, ShipJobDefOf.FlyAway);
					Messages.Message("MessagePermitTransportDrop".Translate(__instance.faction.Named("FACTION")), new LookTargets(cell, __instance.map), MessageTypeDefOf.NeutralEvent);
					__instance.caller.royalty.GetPermit(__instance.def, __instance.faction).Notify_Used();
					if (!__instance.free)
					{
						__instance.caller.royalty.TryRemoveFavor(__instance.faction, __instance.def.royalAid.favorCost);
					}
				}
				return false;
			}
			return true;
		}
	}
}