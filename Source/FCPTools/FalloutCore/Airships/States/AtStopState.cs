using RimWorld.Planet;
using UnityEngine;

namespace FCP.Core;

public class AtStopState : AirshipState
{
    private int arrivedTick;
    private WorldObject currentStop;

    public AtStopState() { } // For loading
    public AtStopState(Airship airship) : base(airship) { }

    public override void OnEnter()
    {
        arrivedTick = GenTicks.TicksGame;
        currentStop = airship.Route.CurrentLeg.toObject;
        airship.Route.CompleteCurrentLeg();
        airship.Tile = currentStop.Tile;

        airship.Route.OnArriveAtStop(airship, currentStop);

        FCPLog.Verbose($"Airship docked at {currentStop?.Label ?? "Unknown"}");
    }

    public override void OnExit()
    {
        currentStop = null;
    }

    public override Vector3 GetDrawPosition()
    {
        // TODO Offset slightly from settlement center
        return currentStop?.DrawPos ?? Find.WorldGrid.NorthPolePos;
    }

    public override void Tick(int delta)
    {
        int dwellTicks = airship.Route.GetDwellTicks(currentStop);
        if (GenTicks.TicksGame < arrivedTick + dwellTicks)
            return;

        if (airship.Route.HasNextLeg())
        {
            FCPLog.Verbose("Airship departing settlement");
            airship.TransitionToState(new TravellingState(airship));
        }
        else if (airship.Route.ShouldLoop)
        {
            FCPLog.Verbose("Airship route looping");
            airship.Route.OnRouteComplete(airship);

            // After OnRouteComplete, the route may have rebuilt legs
            if (airship.Route.HasNextLeg())
                airship.TransitionToState(new TravellingState(airship));
            else
                airship.TransitionToState(new IdleState(airship));
        }
        else
        {
            FCPLog.Verbose("Airship route complete, going idle");
            airship.TransitionToState(new IdleState(airship));
        }
    }

    public override void ExposeData()
    {
        Scribe_Values.Look(ref arrivedTick, "arrivalTick", -1);
        Scribe_References.Look(ref currentStop, "currentSettlement");
    }
}
