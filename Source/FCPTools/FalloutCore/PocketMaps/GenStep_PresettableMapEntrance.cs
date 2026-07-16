using RimWorld;
using Verse;

namespace FCP.PocketMaps
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
