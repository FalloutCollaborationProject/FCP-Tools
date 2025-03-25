using Verse;
using RimWorld;

namespace FCP.Core
{
    public class QueuedMercenaryArrival : IExposable
    {
        public Faction faction;
        public PawnKindDef pawnKind;
        public int arrivalTick;
        public Map destinationMap = null;

        public void ExposeData()
        {
            Scribe_References.Look(ref faction, "faction");
            Scribe_Defs.Look(ref pawnKind, "pawnKind");
            Scribe_Values.Look(ref arrivalTick, "arrivalTick");
            Scribe_References.Look(ref destinationMap, "destinationMap");
        }
    }
}