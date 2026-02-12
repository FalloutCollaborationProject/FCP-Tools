using RimWorld.Planet;
using UnityEngine;

namespace FCP.Core;

public class Airship : WorldObject
{
    private AirshipState currentState = null;
    private AirshipRoute route = new AirshipRoute();

    public AirshipRoute Route => route;
    public override Vector3 DrawPos => currentState?.GetDrawPosition() ?? Find.WorldGrid.NorthPolePos;
    
    public void TransitionToState(AirshipState newState)
    {
        currentState?.OnExit();
        currentState = newState;
        currentState.OnEnter();
    }

    /// <summary>
    /// Set a new route for the Airship
    /// </summary>
    /// <param name="objects">WorldObjects to route through, Tile will be set to the first object to start</param>
    public void SetRoute(IEnumerable<WorldObject> objects)
    {
        var newRoute = new AirshipRoute();

        WorldObject prev = null;
        foreach (WorldObject obj in objects)
        {
            // First settlement - set initial position
            if (prev == null)
                Tile = obj.Tile;
            else
                newRoute.AddLeg(prev, obj);

            prev = obj;
        }

        route = newRoute;
        route.StartRoute();
    }

    public void StartJourney()
    {
        if (route.HasNextLeg())
        {
            // Start Travelling
            TransitionToState(new TravellingState(this));
        }
        else
        {
            FCPLog.Warning("Attempted to start journey with no or empty route");
        }
    }
    
    protected override void TickInterval(int delta)
    {
        base.TickInterval(delta);
        currentState?.Tick(delta);
    }
    
    public override void ExposeData()
    {
        base.ExposeData();
        
        Scribe_Deep.Look(ref route, "route");
        Scribe_Deep.Look(ref currentState, "state");

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            currentState?.PostLoadInit(loadAirship: this);
        }
    }

}
