using RimWorld.Planet;

namespace FCP.Core;

[PublicAPI]
public class WorldComponent_AirshipManager : WorldComponent
{
    public static WorldComponent_AirshipManager Instance { get; private set; }

    private readonly List<Airship> activeAirships = [];
    public IReadOnlyList<Airship> Airships => activeAirships;

    public WorldComponent_AirshipManager(World world) : base(world)
    {
        Instance = this;
    }

    public override void FinalizeInit(bool fromLoad)
    {
        base.FinalizeInit(fromLoad);
        Instance = this;
        RebuildRegistry();
    }

    private void RebuildRegistry()
    {
        activeAirships.Clear();
        foreach (WorldObject obj in Find.WorldObjects.AllWorldObjects)
        {
            if (obj is Airship airship)
                activeAirships.Add(airship);
        }
    }

    public void Register(Airship airship)
    {
        if (!activeAirships.Contains(airship))
            activeAirships.Add(airship);
    }

    public void Deregister(Airship airship)
    {
        activeAirships.Remove(airship);
    }

    /// <summary>Returns the closest airship to a tile for a given faction.</summary>
    public Airship FindNearest(int tile, Faction faction)
    {
        Airship nearest = null;
        float bestDist = float.MaxValue;

        foreach (Airship airship in activeAirships)
        {
            if (airship.Faction != faction)
                continue;

            float dist = Find.WorldGrid.ApproxDistanceInTiles(airship.Tile, tile);
            if (dist < bestDist)
            {
                bestDist = dist;
                nearest = airship;
            }
        }

        return nearest;
    }

    /// <summary>Returns an idle or at-stop airship for a given faction.</summary>
    public Airship FindAvailable(Faction faction)
    {
        foreach (Airship airship in activeAirships)
        {
            if (airship.Faction == faction && airship.IsIdle)
                return airship;
        }

        return null;
    }

    /// <summary>
    /// Find the best candidate airship and send it to a destination with a route of the given type.
    /// </summary>
    public bool RequestAirship(WorldObject destination, Faction faction, AirshipRoute route)
    {
        Airship airship = FindAvailable(faction) ?? FindNearest(destination.Tile, faction);
        if (airship == null)
            return false;

        route.AddLeg(Find.WorldObjects.WorldObjectAt<WorldObject>(airship.Tile) ?? destination, destination);
        airship.AssignRoute(route);
        return true;
    }
}
