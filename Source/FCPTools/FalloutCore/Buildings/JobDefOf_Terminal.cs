using RimWorld;
using Verse;

namespace FCP.Core.Buildings
{
    [DefOf]
    public static class JobDefOf_Terminal
    {
        public static JobDef FCP_ExtractHolotape;
        public static JobDef FCP_HackTerminal;
        public static JobDef FCP_InsertHolotape;
        public static JobDef FCP_ReadHolotapeAtTerminal;
        public static JobDef FCP_LinkPipboyToTerminal;
        public static JobDef FCP_LoadHolotapeIntoPipboy;

        static JobDefOf_Terminal()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(JobDefOf_Terminal));
        }
    }
}