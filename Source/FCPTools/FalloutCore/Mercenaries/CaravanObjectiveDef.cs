using Verse;
using System.Collections.Generic;

namespace FCP.Core
{
    public class CaravanObjectiveDef : Def
    {
        public List<ThingDef> tradeTags = new List<ThingDef>(); // Objectives by trade tags
        public List<PawnKindDef> prisonerPawnKinds = new List<PawnKindDef>(); // Objectives by prisoner pawn kinds
        public float successRate = 0.75f; // Default success rate 75%
    }
}
