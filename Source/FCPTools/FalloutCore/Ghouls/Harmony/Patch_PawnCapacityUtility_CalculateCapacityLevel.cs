using HarmonyLib;
using System.Reflection;

namespace FCP.Core.Ghouls;

[HarmonyPatch(typeof(PawnCapacityUtility), nameof(PawnCapacityUtility.CalculateCapacityLevel))]
public static class Patch_PawnCapacityUtility_CalculateCapacityLevel
{
    public static void Prefix(HediffSet diffSet, PawnCapacityDef capacity, ref List<PawnCapacityUtility.CapacityImpactor> impactors, bool forTradePrice)
    {
        if (impactors == null || diffSet?.pawn == null)
            return;
                
        if (!diffSet.pawn.health.hediffSet.HasHediff(HediffDefOf_Ghoul.ToxicHealing))
            return;

        impactors.RemoveAll(impactor =>
        {
            FieldInfo hediffField = AccessTools.Field(impactor.GetType(), "hediff");
            if (hediffField == null) 
                return false;
            
            var hediff = hediffField.GetValue(impactor) as Hediff;
            return hediff?.def == HediffDefOf.ToxicBuildup;
        });
    }
}