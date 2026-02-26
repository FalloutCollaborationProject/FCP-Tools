using RimWorld.Planet;
using UnityEngine;

namespace FCP.Core;

[UsedImplicitly]
public class Airship : WorldObject
{
    private AirshipState currentState = null;
    private AirshipRoute route = new AirshipRoute();
    private AirshipTweener tweener;

    public AirshipRoute Route => route;
    public Vector3 TargetPos => currentState?.GetDrawPosition() ?? Find.WorldGrid.NorthPolePos;
    public override Vector3 DrawPos => tweener?.TweenedPos ?? TargetPos;
    
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
    
    public override void SpawnSetup()
    {
        base.SpawnSetup();
        tweener = new AirshipTweener(this);
        tweener.ResetToTarget();
    }

    protected override void TickInterval(int delta)
    {
        base.TickInterval(delta);
        currentState?.Tick(delta);
        tweener?.TweenerTickInterval(delta);
    }
    
    public override void DrawExtraSelectionOverlays()
    {
        base.DrawExtraSelectionOverlays();

        Vector3 prev = DrawPos;

        // Line to current leg destination
        if (route.CurrentLeg?.toObject != null)
        {
            Vector3 next = Find.WorldGrid.GetTileCenter(route.CurrentLeg.ToTile);
            DrawWorldArc(prev, next);
            prev = next;
        }

        // Lines between remaining stops
        foreach (RouteLeg leg in route.RemainingLegs)
        {
            Vector3 next = Find.WorldGrid.GetTileCenter(leg.ToTile);
            DrawWorldArc(prev, next);
            prev = next;
        }
    }

    /// <summary>
    /// Draws an arc along the planet surface between two world positions,
    /// subdivided into small slerp segments so it follows the sphere curvature.
    /// </summary>
    private static void DrawWorldArc(Vector3 from, Vector3 to, float altitudeOffset = 0.08f)
    {
        float distance = GenMath.SphericalDistance(from.normalized, to.normalized);
        int segments = Mathf.Max(Mathf.CeilToInt(distance / 0.01f), 1);

        Vector3 prev = from + from.normalized * altitudeOffset;
        for (int i = 1; i <= segments; i++)
        {
            float t = (float)i / segments;
            Vector3 point = Vector3.Slerp(from, to, t);
            point += point.normalized * altitudeOffset;
            GenDraw.DrawWorldLineBetween(prev, point);
            prev = point;
        }
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
