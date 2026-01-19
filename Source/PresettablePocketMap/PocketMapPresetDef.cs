using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PresettablePocketMap
{
    public class PocketMapPresetDef : Def
    {
        public List<PocketMapPresetRow> rows;
        public int width;
        public int height;
        public TerrainDef floor;
        public ThingDef wallDef;
        public ThingDef wallStuff;
        public IntVec3 exitPosition = new IntVec3(1, 0, 1);
    }
}
