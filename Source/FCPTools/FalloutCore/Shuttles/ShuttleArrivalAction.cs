using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using UnityEngine;

namespace FCP.Core.Shuttles
{
    public static class ShuttleArrivalAction
    {
        public static void Arrive(List<Thing> things, Map map, Faction faction, FactionModExtension extension, IntVec3 spawnCenter)
        {
            List<IntVec3> previousLandingSpots = new List<IntVec3>();
            int maxThingsPerShuttle = extension.maxPawnCountInOneShuttle;
            if (maxThingsPerShuttle <= 0)
            {
                maxThingsPerShuttle = things.Count;
            }

            for (int i = 0; i < things.Count; i += maxThingsPerShuttle)
            {
                List<Thing> currentThings = things.GetRange(i, Mathf.Min(maxThingsPerShuttle, things.Count - i));
                Thing shuttleThing = ThingMaker.MakeThing(extension.transportShipDef.shipThing);
                shuttleThing.SetFaction(faction);
                TransportShip transportShip = TransportShipMaker.MakeTransportShip(extension.transportShipDef, currentThings, shuttleThing);
                IntVec3 landingSpot;
                int tries = 0;
                do
                {
                    if (!CellFinder.TryFindRandomReachableNearbyCell(spawnCenter, map,
                    tries + extension.minDistanceBetweenShuttles,
                    TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false),
                    (IntVec3 c) => !c.Roofed(map) && !c.Fogged(map), null, out landingSpot))
                    {
                        // Fallback
                        DropCellFinder.TryFindDropSpotNear(MapGenerator.PlayerStartSpot, map, out landingSpot, false, false);
                    }
                    tries++;
                    if (tries > 1000)
                    {
                        Log.Error("FCP.Core.Shuttles: Could not find a suitable landing spot after 1000 tries.");
                        landingSpot = MapGenerator.PlayerStartSpot;
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
}
