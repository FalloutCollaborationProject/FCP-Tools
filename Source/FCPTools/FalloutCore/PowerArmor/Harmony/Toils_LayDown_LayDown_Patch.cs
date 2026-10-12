using HarmonyLib;
using Verse.AI;

namespace FCP.Core.PowerArmor;

[HarmonyPatch(typeof(Toils_LayDown), nameof(Toils_LayDown.LayDown))]
public static class Toils_LayDown_LayDown_Patch
{
    public static void Postfix(Toil __result)
    {
        Toil toil = __result;
        toil.AddPreInitAction(delegate
        {
            Pawn pawn = toil.actor;
            if (pawn?.apparel == null || pawn.Downed || pawn.Faction == null || !pawn.Faction.IsPlayer)
                return;

            List<Apparel> worn = pawn.apparel.WornApparel;
            for (int i = worn.Count - 1; i >= 0; i--)
            {
                var props = worn[i].def.GetCompProperties<CompProperties_PowerArmor>();
                if (props != null && !props.canSleep)
                    pawn.apparel.TryDrop(worn[i], out _, pawn.PositionHeld, false);
            }
        });
    }
}
