using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace PresettablePocketMap
{
    public class GenStep_PresettableMapEntrance : GenStep
    {
        public int x;
        public int y;
        public override int SeedPart => 928734;

        public override void Generate(Map map, GenStepParams parms)
        {
            MapGenerator.PlayerStartSpot = new IntVec3(x, 0, y);
        }
    }
}
