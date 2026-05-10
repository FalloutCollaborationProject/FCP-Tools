using RimWorld;
using Verse;

namespace FCP.Core.Buildings;

[DefOf]
public static class JobDefOf
{
    public static JobDef FCP_StrapToPowerLine;
    public static JobDef FCP_RemoveFromPowerLine;
    
    static JobDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(JobDefOf));
}
