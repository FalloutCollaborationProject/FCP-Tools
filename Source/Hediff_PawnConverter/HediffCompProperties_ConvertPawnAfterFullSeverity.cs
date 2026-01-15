using RimWorld;
using Verse;
using System.Collections.Generic;

namespace Hediff_PawnConverter
{
    public class HediffCompProperties_ConvertPawnAfterFullSeverity : HediffCompProperties
    {
        public PawnKindDef pawnKindDef;

        public List<PawnKindDef> pawnKindDefs;

        public FactionDef factionDef;
       
        public List<XenotypeDef> immuneXenotypeDef;

        public List<HediffDef> immuneHediffDef;

        public bool isUsingImmunityList;

        public float severityToTransform = 1f;

        public bool removeIfImmune = false;

        public bool isInfectiousToMechanoid = false;

        public bool isPlayer = false;

        public int numberToSpawn = 1;

        public bool inheritName = false;

        public bool inheritAge = false;

        public bool inheritBackground = false;

        public bool inheritSkills = false;
        public HediffCompProperties_ConvertPawnAfterFullSeverity()
        {
            compClass = typeof(HediffCompConvertPawnAfterFullSeverity);
        }
    }
}
