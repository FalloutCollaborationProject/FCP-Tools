using RimWorld.Planet;
using UnityEngine;

namespace FCP.Core;

public class IdleState : AirshipState
{
    private WorldObject dockedAt;

    public IdleState() { } // For loading
    public IdleState(Airship airship) : base(airship) { }
    
    public override void OnEnter()
    {
        dockedAt = airship.Route.LastStop;
        if (dockedAt != null)
            airship.Tile = dockedAt.Tile;
        FCPLog.Verbose($"Airship idle at {dockedAt?.Label ?? "Unknown"}");
    }

    public override void OnExit()
    {
        
    }

    public override Vector3 GetDrawPosition()
    {
        return dockedAt?.DrawPos ?? Find.WorldGrid.NorthPolePos;
    }

    public override void Tick(int delta)
    {
        // Idle - waiting for new orders
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref dockedAt, "dockedAt");
    }
}