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

    public void AppendDestination(WorldObject destination)
    {
        route.AppendLeg(destination);
        if (currentState is IdleState)
        {
            route.StartRoute();
            TransitionToState(new TravellingState(this));
        }
        else if (currentState is AtStopState && route.CurrentLeg == null)
        {
            // Appended a leg while docked at the last stop — populate currentLeg so
            // the dwell timer (or Depart Now) can find it.
            route.StartRoute();
        }
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (Gizmo g in base.GetGizmos()) 
            yield return g;
        
        if (!Verse.DebugSettings.godMode) 
            yield break;

        // Arrive Now
        {
            var cmd = new Command_Action
            {
                defaultLabel = "Dev: Arrive Now",
                defaultDesc = "Force-complete the current travel leg immediately.",
                action = () =>
                {
                    TransitionToState(new AtStopState(this));
                    tweener?.ResetToTarget();
                }
            };
            if (currentState is not TravellingState)
                cmd.Disable("Not currently travelling.");
            yield return cmd;
        }
        // Depart Now
        {
            var cmd = new Command_Action
            {
                defaultLabel = "Dev: Depart Now",
                defaultDesc = "Skip dwell time and immediately depart the current stop.",
                action = () =>
                {
                    if (route.HasNextLeg())
                        TransitionToState(new TravellingState(this));
                    else
                        TransitionToState(new IdleState(this));
                    tweener?.ResetToTarget();
                }
            };
            if (currentState is not AtStopState)
                cmd.Disable("Not currently docked at a stop.");
            yield return cmd;
        }
        // Add Destination
        yield return new Command_Action
        {
            defaultLabel = "Dev: Add Destination",
            defaultDesc = "Click a settlement on the world map to append it to the route.",
            action = () =>
            {
                Find.WorldTargeter.BeginTargeting(
                    action: target =>
                    {
                        if (target.WorldObject is Settlement settlement)
                        {
                            AppendDestination(settlement);
                            return true;
                        }
                        Messages.Message("Must select a settlement.", MessageTypeDefOf.RejectInput, historical: false);
                        return false;
                    },
                    canTargetTiles: false,
                    mouseAttachment: null,
                    closeWorldTabWhenFinished: false,
                    onUpdate: null,
                    extraLabelGetter: null
                );
            }
        };
        // Log State
        yield return new Command_Action
        {
            defaultLabel = "Dev: Log State",
            defaultDesc = "Print airship state info to the dev console.",
            action = () =>
            {
                string stateInfo = currentState switch
                {
                    TravellingState ts => $"Travelling ({ts.TravelProgress * 100f:F1}%)",
                    AtStopState => "AtStop",
                    IdleState => $"Idle at {route.LastStop?.Label ?? "Unknown"}",
                    null => "No state",
                    _ => currentState.GetType().Name
                };
                int legsLeft = route.RemainingLegs.Count();
                FCPLog.Message($"[Airship] State: {stateInfo} | Legs remaining: {legsLeft}");
            }
        };
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
