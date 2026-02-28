using RimWorld.Planet;

namespace FCP.Core;

/// <summary>
/// Simple A->B route. Default for general use.
/// </summary>
public class DirectRoute : AirshipRoute
{
    public override int GetDwellTicks(WorldObject stop) => GenDate.TicksPerHour * 5;

    public override void OnArriveAtStop(Airship airship, WorldObject stop) { }

    public override void OnRouteComplete(Airship airship) { }
}
