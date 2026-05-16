using HarmonyLib;
using Verse.AI;

namespace FCP.Core.WeaponCondition;

[HarmonyPatch(typeof(Verb_Shoot), "TryCastShot")]
public static class VerbShoot_TryCastShot_Patch
{
    public static bool Prefix(Verb_Shoot __instance, ref bool __result)
    {
        if (!FCPCoreMod.Settings.General.weaponConditionEnabled)
            return true;

        ThingWithComps weapon = __instance.EquipmentSource as ThingWithComps;
        if (weapon == null)
            return true;

        CompWeaponCondition comp = weapon.GetComp<CompWeaponCondition>();
        if (comp == null)
            return true;

        if (comp.IsDisabled)
        {
            __result = false;
            return false;
        }

        Pawn wielder = __instance.CasterPawn;
        if (comp.IsJammed && comp.Condition >= comp.Props.jamThreshold)
            comp.ClearJam();
        if (comp.IsJammed || comp.TryJam(wielder))
        {
            __result = false;
            if (wielder != null && wielder.CurJobDef != WeaponConditionDefOf.FCP_Job_ClearWeaponJam)
                wielder.jobs.StartJob(
                    JobMaker.MakeJob(WeaponConditionDefOf.FCP_Job_ClearWeaponJam),
                    JobCondition.InterruptForced, null,
                    resumeCurJobAfterwards: true, cancelBusyStances: false);
            return false;
        }

        return true;
    }

    public static void Postfix(Verb_Shoot __instance, bool __result)
    {
        if (!__result || !FCPCoreMod.Settings.General.weaponConditionEnabled)
            return;

        (__instance.EquipmentSource as ThingWithComps)?.GetComp<CompWeaponCondition>()?.NotifyShot();
    }
}
