using System.Collections.Generic;
using RimWorld;
using Verse;

namespace FCP.Core.Robotics
{
    public class GenStep_WildRobotSpawn : GenStep
    {
        public float chancePerMap = 0.06f;
        public List<PawnKindDef> kinds = new List<PawnKindDef>();

        public override int SeedPart => 1927481158;

        public override void Generate(Map map, GenStepParams parms)
        {
            if (kinds.NullOrEmpty() || !Rand.Chance(chancePerMap))
            {
                return;
            }

            Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kinds.RandomElement(), faction: null));
            if (pawn == null)
            {
                return;
            }

            if (!CellFinder.TryFindRandomCellNear(map.Center, map, 50, c => c.Standable(map) && !c.Fogged(map), out IntVec3 cell))
            {
                cell = CellFinder.RandomCell(map);
            }

            GenSpawn.Spawn(pawn, cell, map);

            CompRefuelable fuel = pawn.GetComp<CompRefuelable>();
            if (fuel != null && fuel.Fuel > 0f)
            {
                fuel.ConsumeFuel(fuel.Fuel);
            }
        }
    }
}
