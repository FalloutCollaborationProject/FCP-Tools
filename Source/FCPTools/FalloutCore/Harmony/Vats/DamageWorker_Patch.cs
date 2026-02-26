using HarmonyLib;

namespace FCP.Core.Vats;

[HarmonyPatch(typeof(DamageWorker_AddInjury))]
public static class DamageWorker_Patch
{
    private static readonly AccessTools.StructFieldRef<DamageInfo, BodyPartRecord> HitPartRef =
        AccessTools.StructFieldRefAccess<DamageInfo, BodyPartRecord>("hitPartInt");
    
    private static readonly AccessTools.StructFieldRef<DamageInfo, bool> AllowDamagePropagationRef =
        AccessTools.StructFieldRefAccess<DamageInfo, bool>("allowDamagePropagationInt");
    
    [HarmonyPrefix]
    [HarmonyPatch("ApplyToPawn")]
    public static bool ApplyToPawn_Patch(ref DamageInfo dinfo, Pawn pawn)
    {
        if (dinfo.Instigator is not Pawn instigator)
        {
            return true;
        }

        // Check if this attack is one launched from VATS
        if (VATS_GameComponent.ActiveAttacks.TryGetValue(instigator, out VATS_GameComponent.VATSAction attack) && attack.Target == pawn)
        {
            HitPartRef(ref dinfo) = attack.Part;
            AllowDamagePropagationRef(ref dinfo) = false;
        }
        
        // Apply any applicable legendary effects
        foreach (LegendaryWeaponTraitDef legendaryEffectDef in GetLegendaryEffectsFor(instigator))
        {
            legendaryEffectDef.LegendaryEffectWorker.Notify_ApplyToPawn(ref dinfo, pawn);
        }
        return true;
    }

    public static IEnumerable<LegendaryWeaponTraitDef> GetLegendaryEffectsFor(Pawn pawn)
    {
        if (
            pawn.equipment?.Primary != null
            && pawn.equipment.Primary.TryGetComp(out CompUniqueWeapon uniqueWeapon))
        {
            foreach (LegendaryWeaponTraitDef trait in uniqueWeapon.TraitsListForReading.OfType<LegendaryWeaponTraitDef>())
            {
                yield return trait;
            }
        }
    }
}