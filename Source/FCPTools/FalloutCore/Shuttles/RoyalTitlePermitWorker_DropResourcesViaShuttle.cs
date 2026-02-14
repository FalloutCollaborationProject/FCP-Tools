using HarmonyLib;

namespace FCP.Core.Shuttles;

[HarmonyPatch(typeof(RoyalTitlePermitWorker_DropResources), "CallResources")]
public static class RoyalTitlePermitWorker_DropResources_Patch
{
	public static bool Prefix(RoyalTitlePermitWorker_DropResources __instance, IntVec3 cell, 
		Faction ___faction, Map ___map, Pawn ___caller, bool ___free)
	{
		var extension = ___faction.def.GetModExtension<FactionModExtension>();
			
		if (extension?.transportShipDef == null) 
			return true;
			
		var contents = new List<Thing>();
		foreach (ThingDefCountClass defCount in __instance.def.royalAid.itemsToDrop)
		{
			var thing = ThingMaker.MakeThing(defCount.thingDef);
			thing.stackCount = defCount.count;
			contents.Add(thing);
		}

		if (!contents.Any()) 
			return false;
			
		Thing shipThing = ThingMaker.MakeThing(extension.transportShipDef.shipThing);
		shipThing.SetFaction(___faction);
			
		TransportShip transportShip = TransportShipMaker.MakeTransportShip(extension.transportShipDef, contents, shipThing);
		transportShip.ArriveAt(cell, ___map.Parent);
		transportShip.AddJobs(ShipJobDefOf.Unload, ShipJobDefOf.FlyAway);
			
		Messages.Message("MessagePermitTransportDrop".Translate(___faction.Named("FACTION")), new LookTargets(cell, ___map), MessageTypeDefOf.NeutralEvent);
		___caller.royalty.GetPermit(__instance.def, ___faction).Notify_Used();
		if (!___free)
		{
			___caller.royalty.TryRemoveFavor(___faction, __instance.def.royalAid.favorCost);
		}
		return false;
	}
}