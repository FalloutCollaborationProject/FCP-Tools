using RimWorld.Planet;
using UnityEngine;

namespace FCP.Core;

public class AtStopState : AirshipState
{
    private int arrivedTick;
    private WorldObject currentStop;
    
    private const int DwellTimeTicks = GenDate.TicksPerHour * 5; // TEMP TODO

    public AtStopState() { } // For loading
    public AtStopState(Airship airship) : base(airship) { }

    public override void OnEnter()
    {
        arrivedTick = GenTicks.TicksGame;
        currentStop = airship.Route.CurrentLeg.toObject;
        airship.Route.CompleteCurrentLeg();
        
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
        if (GenTicks.TicksGame < arrivedTick + DwellTimeTicks)
            return;

        if (airship.Route.HasNextLeg())
        {
            FCPLog.Verbose("Airship departing settlement");
            airship.TransitionToState(new TravellingState(airship));
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