using RimWorld;
using Verse;

namespace FCP.Core.Buildings;

[DefOf]
public static class HediffDefOf
{
    public static HediffDef FCP_Crucified;
    public static HediffDef FCP_WasCrucified;
    public static HediffDef FCP_CrucifixionExhaustion;
    
    static HediffDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(HediffDefOf));
}
