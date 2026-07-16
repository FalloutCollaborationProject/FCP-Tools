using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace FCP.Core.Robotics
{
    public class GenStep_DerelictRobot : GenStep
    {
        public float chancePerMap = 0.06f;
        public List<CharacterDef> characters = new List<CharacterDef>();

        public override int SeedPart => 1927481157;

        public override void Generate(Map map, GenStepParams parms)
        {
            if (characters.NullOrEmpty() || !Rand.Chance(chancePerMap))
            {
                return;
            }

            UniqueCharactersTracker tracker = UniqueCharactersTracker.Instance;
            if (tracker == null)
            {
                return;
            }

            List<CharacterDef> candidates = characters.Where(c => !tracker.CharacterPawnExists(c)).ToList();
            if (candidates.Count == 0)
            {
                return;
            }

            Pawn pawn = tracker.GetOrGenPawn(candidates.RandomElement());
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
