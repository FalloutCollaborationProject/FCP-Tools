using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CompGiveThought
{
    public class CompProperties_GiveThought : CompProperties
    {
        public CompProperties_GiveThought()
        {
            this.compClass = typeof(CompGiveThought);
        }

        public ThoughtDef thoughtDef;
        public int radius = 0;
        public bool enableInInventory = false;
    }
}
