using RimWorld.Planet;

namespace FCP.Core;

/// <summary>
/// Looping patrol route. When complete, rebuilds the route in reverse to patrol back and forth.
/// </summary>
public class PatrolRoute : AirshipRoute
{
    private List<WorldObject> patrolStops = [];

    public PatrolRoute() { }

    public PatrolRoute(List<WorldObject> stops)
    {
        patrolStops = new List<WorldObject>(stops);
    }

    public override int GetDwellTicks(WorldObject stop) => GenDate.TicksPerHour * 3;

    public override void OnArriveAtStop(Airship airship, WorldObject stop) { }

    public override void OnRouteComplete(Airship airship)
    {
        if (patrolStops.Count < 2)
            return;

        // Reverse the stops and rebuild legs
        patrolStops.Reverse();
        for (int i = 0; i < patrolStops.Count - 1; i++)
            AddLeg(patrolStops[i], patrolStops[i + 1]);

        StartRoute();
    }

    public override bool ShouldLoop => true;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref patrolStops, "patrolStops", LookMode.Reference);

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            patrolStops ??= new List<WorldObject>();
        }
    }
}
