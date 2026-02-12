using RimWorld.Planet;

namespace FCP.Core;

/// <summary>
/// Handles the travel queue/path
/// </summary>
public class AirshipRoute : IExposable
{
    private Queue<RouteLeg> legs = new Queue<RouteLeg>();
    private RouteLeg currentLeg;
    private WorldObject lastStop;

    public RouteLeg CurrentLeg => currentLeg;
    public WorldObject LastStop => lastStop;

    public bool HasNextLeg() => legs.Count > 0;
    
    public void AddLeg(WorldObject from, WorldObject to) => legs.Enqueue(new RouteLeg(from, to));

    public void StartRoute()
    {
        if (legs.Count > 0) 
            currentLeg = legs.Dequeue();
        else
            FCPLog.Error("AirshipRoute called StartRoute with empty legs");
    }

    public void CompleteCurrentLeg()
    {
        lastStop = currentLeg?.toObject;

        if (legs.Count > 0)
            currentLeg = legs.Dequeue();
        else
            currentLeg = null;
    }

    public void ExposeData()
    {
        Scribe_Deep.Look(ref currentLeg, "currentLeg");
        Scribe_References.Look(ref lastStop, "lastSettlement");
        Scribe_Collections.Look(ref legs, "legs", LookMode.Deep);
        
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            legs ??= new Queue<RouteLeg>();
        }
    }
}

/// <summary>
/// Individual Stop along a AirshipRoute
/// </summary>
public class RouteLeg : IExposable
{
    public WorldObject fromObject;
    public WorldObject toObject;

    public PlanetTile FromTile => fromObject.Tile;
    public PlanetTile ToTile => toObject.Tile;

    public RouteLeg() { } // For loading

    public RouteLeg(WorldObject from, WorldObject to)
    {
        fromObject = from;
        toObject = to;
    }

    public void ExposeData()
    {
        Scribe_References.Look(ref fromObject, "from");
        Scribe_References.Look(ref toObject, "to");
    }
}