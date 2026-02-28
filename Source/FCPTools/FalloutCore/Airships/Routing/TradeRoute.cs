using RimWorld.Planet;

namespace FCP.Core;

/// <summary>
/// Trade route — lingers longer at the trade target, then goes idle at the end.
/// </summary>
public class TradeRoute : AirshipRoute
{
    private WorldObject tradeTarget;

    public TradeRoute() { }

    public TradeRoute(WorldObject tradeTarget)
    {
        this.tradeTarget = tradeTarget;
    }

    public override int GetDwellTicks(WorldObject stop)
        => stop == tradeTarget ? GenDate.TicksPerHour * 8 : GenDate.TicksPerHour * 2;

    public override void OnArriveAtStop(Airship airship, WorldObject stop)
    {
        if (stop == tradeTarget)
        {
            // TODO: trigger trade interaction (future work)
        }
    }

    public override void OnRouteComplete(Airship airship) { }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref tradeTarget, "tradeTarget");
    }
}
