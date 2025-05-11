using System.Collections.Generic;
using Verse;

namespace FCP_Ghoul
{
    public class FeralityGene_ModExtension : DefModExtension
    {
        public int rate=60;
        public List<ThingDef> drugs = new List<ThingDef>();
        public int amountReduced = 10;
        public float dropApparelChance;
    }
}
