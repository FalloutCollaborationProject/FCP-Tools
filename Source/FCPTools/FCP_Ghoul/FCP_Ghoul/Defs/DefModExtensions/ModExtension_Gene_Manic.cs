using System.Collections.Generic;
using JetBrains.Annotations;
using Verse;

namespace FCP_Ghoul
{
    [UsedImplicitly]
    public class ModExtension_Gene_Manic : DefModExtension
    {
        public int rate = 60;
        public int turnFeralThreshold = 100;
        public int amountReduced = 10;
        public float dropApparelChance = 1f;
        public List<ThingDef> drugs = [];
    }
}