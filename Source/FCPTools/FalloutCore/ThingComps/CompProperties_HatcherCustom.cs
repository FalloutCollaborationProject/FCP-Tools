namespace FCP.Core
{
    public class CompProperties_HatcherCustom : CompProperties
    {
        public float hatcherDaystoHatch = 1f;

        public PawnKindDef hatcherPawn;
        public float secondaryOverrideChance = 0f;
        public PawnKindDef secondaryPawn;


        public CompProperties_HatcherCustom()
        {
            compClass = typeof(CompHatcherCustom);
        }
    }
}
