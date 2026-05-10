using Verse;

namespace FCP.Core.Buildings
{
    public class CompProperties_Terminal : CompProperties
    {
        public string poweredOnTexPath;
        public string poweredOffTexPath;
        public bool canExtractHolotape;
        public float holotapeDropChance = 0.1f;

        public CompProperties_Terminal()
        {
            compClass = typeof(CompTerminal);
        }
    }
}