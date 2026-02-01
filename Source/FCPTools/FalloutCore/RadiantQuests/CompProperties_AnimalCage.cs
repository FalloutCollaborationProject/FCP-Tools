using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FCP.Core.RadiantQuests
{
    public class CompProperties_AnimalCage : CompProperties_Refuelable
    {
        public float maxBodySize = 1f;
        public float minBodySize = 0f;
        public int ticksForCaptureChance = 60000;
        public float captureChance = 0.5f; 
        public List<PawnKindDef> animalsThatGetCaught = new List<PawnKindDef>();
        public CompProperties_AnimalCage()
        {
            compClass = typeof(CompAnimalCage);
        }
    }
}

