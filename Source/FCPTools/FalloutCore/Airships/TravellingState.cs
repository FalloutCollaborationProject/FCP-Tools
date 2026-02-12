using UnityEngine;

namespace FCP.Core;

public class TravellingState : AirshipState
{
    private Vector3 startPos;
    private Vector3 endPos;
    private float travelProgress;
    private const float TravelSpeed = 0.00025f;

    public TravellingState() { } // For loading
    public TravellingState(Airship airship) : base(airship) { }

    public override void OnEnter()
    {
        AirshipRoute route = airship.Route;
        
        startPos = Find.WorldGrid.GetTileCenter(route.CurrentLeg.FromTile);
        endPos = Find.WorldGrid.GetTileCenter(route.CurrentLeg.ToTile);
        travelProgress = 0f;
        
        FCPLog.Verbose($"Airship departing: {route.CurrentLeg.fromObject?.Label ?? "Unknown"} " +
                       $"-> {route.CurrentLeg.fromObject?.Label ?? "Unknown"}");
    }

    public override void OnExit()
    {
        travelProgress = 1f;
    }

    public override Vector3 GetDrawPosition()
    {
        return Vector3.Slerp(startPos, endPos, travelProgress);
    }

    public override void Tick(int delta)
    {
        travelProgress += GetTravelStepPerTick() * delta;
        if (travelProgress < 1f) return;
        
        travelProgress = 1f;
        FCPLog.Verbose($"Airship arrived at {airship.Route.CurrentLeg.toObject?.Label ?? "Unknown"}");
        airship.TransitionToState(new AtStopState(airship));
    }

    private float GetTravelStepPerTick()
    {
        if (startPos == endPos) return 1f;
        
        float distance = GenMath.SphericalDistance(startPos.normalized, endPos.normalized);
        return distance == 0f ? 1f : TravelSpeed / distance;
    }

    public override void ExposeData()
    {
        Scribe_Values.Look(ref startPos, "travelStartPos");
        Scribe_Values.Look(ref endPos, "travelEndPos");
        Scribe_Values.Look(ref travelProgress, "travelProgress");
    }
}