using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PresettablePocketMap
{
    public class PocketMapPresetCell
    {
        public List<PocketMapPresetBuilding> buildings = new List<PocketMapPresetBuilding>();
        public List<PocketMapPresetPawnKind> pawnKinds = new List<PocketMapPresetPawnKind>();
        public TerrainDef foundation;
        public TerrainDef floor;
        public int x;
        public int y;
    }
}
