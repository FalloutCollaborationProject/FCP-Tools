using RimWorld.Planet;

namespace FCP.Core;

/// <summary>
/// Handles the travel queue/path. Subclasses define dwell times and arrival/completion behavior.
/// </summary>
public abstract class AirshipRoute : IExposable
{
    private Queue<RouteLeg> legs = new Queue<RouteLeg>();
    private RouteLeg currentLeg;
    private WorldObject lastStop;

    public RouteLeg CurrentLeg => currentLeg;
    public WorldObject LastStop => lastStop;

    public bool HasNextLeg() => currentLeg != null || legs.Count > 0;
    public IEnumerable<RouteLeg> RemainingLegs => legs;

    /// <summary>How long the airship should dwell at a given stop, in ticks.</summary>
    public abstract int GetDwellTicks(WorldObject stop);

    /// <summary>Called when the airship arrives at a stop.</summary>
    public abstract void OnArriveAtStop(Airship airship, WorldObject stop);

    /// <summary>Called when the route has no more legs.</summary>
    public abstract void OnRouteComplete(Airship airship);

    /// <summary>If true, the route loops when complete.</summary>
    public virtual bool ShouldLoop => false;

    public void AddLeg(WorldObject from, WorldObject to) => legs.Enqueue(new RouteLeg(from, to));

    /// <summary>Append a new leg to the end of the queue.</summary>
    public void AppendLeg(WorldObject to)
    {
        WorldObject from = legs.Any() ? legs.Last().toObject
                         : currentLeg?.toObject
                         ?? lastStop;
        if (from == null)
        {
            FCPLog.Warning("AppendLeg: no known 'from' position, cannot append leg");
            return;
        }
        legs.Enqueue(new RouteLeg(from, to));
    }

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

    public virtual void ExposeData()
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
