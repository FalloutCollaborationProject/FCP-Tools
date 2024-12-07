using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI.Group;
using Verse.Noise;

namespace FCP_RadiantQuests
{
    public class QuestPart_SpawnSettlers: QuestPart
    {
        public string inSignal;

        public List<Pawn> pawns;

        public int mapTile;

        public IntVec3 cell = IntVec3.Invalid;
        public int radius = 20;

        private bool spawned;
        private MapParent mapParent;
        public override IEnumerable<GlobalTargetInfo> QuestLookTargets
        {
            get
            {
                foreach (GlobalTargetInfo questLookTarget in base.QuestLookTargets)
                {
                    yield return questLookTarget;
                }

            }
        }


        public override void Notify_QuestSignalReceived(Signal signal)
        {
            mapParent = Find.World.worldObjects.MapParentAt(mapTile);
            base.Notify_QuestSignalReceived(signal);
            Log.Message("Testing notify quest signal received");
            if (!(signal.tag == inSignal) || !mapParent.HasMap)
            {
                return;
            }
            IntVec3 location = IntVec3.Invalid;
            Log.Message(mapParent.Map == null);
            Log.Message(pawns.Count);
            if (cell.IsValid)
            {
                location = cell;
            }
            else
            {
                location = mapParent.Map.Center;
            
            }
            foreach (Pawn pawn in pawns)
            {
                if (!RCellFinder.TryFindRandomCellNearWith(location, (IntVec3 c) => c.Standable(mapParent.Map), mapParent.Map, out var result, radius))
                {
                    break;
                }
                GenSpawn.Spawn(pawn, result, mapParent.Map);
            }
            Lord lord = LordMaker.MakeNewLord(pawns[0].Faction, new LordJob_DefendPoint(mapParent.Map.Center, radius, radius, false, false), mapParent.Map, pawns);

            spawned = true;

        }

        public override bool QuestPartReserves(Pawn p)
        {
            return false;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref inSignal, "inSignal");
            Scribe_Values.Look(ref spawned, "spawned", defaultValue: false);
            Scribe_Collections.Look(ref pawns, "pawns", LookMode.Reference);
            Scribe_References.Look(ref mapParent, "mapParent");
            Scribe_Values.Look(ref cell, "cell");

        }

        public override void AssignDebugData()
        {
            base.AssignDebugData();
            inSignal = "DebugSignal" + Rand.Int;
            if (Find.AnyPlayerHomeMap != null)
            {
                mapParent = Find.RandomPlayerHomeMap.Parent;
                pawns = new List<Pawn>() { PawnGenerator.GeneratePawn(PawnKindDefOf.Beggar)};
            }
        }
    }
}
