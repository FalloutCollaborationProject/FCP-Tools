using RimWorld;
using Verse;
using Verse.AI.Group;

namespace FCP.PocketMaps;

public class LordJob_StationedResidents : LordJob
{
    private IntVec3 baseCenter;

    public LordJob_StationedResidents()
    {
    }

    public LordJob_StationedResidents(IntVec3 baseCenter)
    {
        this.baseCenter = baseCenter;
    }

    public override StateGraph CreateGraph()
    {
        StateGraph stateGraph = new StateGraph();
        stateGraph.StartingToil = new LordToil_DefendBase(baseCenter);
        return stateGraph;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref baseCenter, "baseCenter");
    }
}
