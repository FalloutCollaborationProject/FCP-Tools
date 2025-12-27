using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RecipeGoodwillLimiter
{
    public class RecipeExtension_GoodwillCheck : DefModExtension
    {
        public FactionDef requireFaction;

        public int minimumGoodwill = -1;

        public bool uncraftableIfFactionDefeated = true;
    }
}
