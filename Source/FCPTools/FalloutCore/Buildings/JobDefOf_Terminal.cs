using RimWorld;
using Verse;

namespace FCP.Core.Buildings
{
    [DefOf]
    public static class JobDefOf_Terminal
    {
        public static JobDef FCP_ExtractHolotape;

        static JobDefOf_Terminal()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(JobDefOf_Terminal));
        }
    }
}