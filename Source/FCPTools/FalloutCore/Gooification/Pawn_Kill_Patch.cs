using HarmonyLib;

namespace FCP.Core;

public class GooificationState
{
    public List<Thing> Drops;
    public IntVec3 Position;
    public Map Map;
}

[HarmonyPatch(typeof(Pawn), "Kill")]
public static class Pawn_Kill_Patch
{
    public static void Prefix(Pawn __instance, DamageInfo? dinfo, Hediff exactCulprit, out GooificationState __state)
    {
        __state = null;
        if (dinfo.HasValue is false && DamageWithFilth.curDinfo.TryGetValue(__instance, out var oldDinfo))
        {
            dinfo = oldDinfo;
        }
        if (dinfo.HasValue)
        {
            var weapon = dinfo.Value.Weapon;
            if (weapon != null)
            {
                var extension = weapon.GetModExtension<DeathEffectModExtension>();
                if (extension != null && dinfo.Value.Def?.Worker is DamageWithFilth damage
                    && Rand.Chance(extension.effectChance))
                {
                    var pawn = __instance;
                    Thing thing = ThingMaker.MakeThing(ThingDef.Named(damage.FilthToSpawn));
                    GenSpawn.Spawn(thing, pawn.Position, pawn.Map, WipeMode.Vanish);
                    __state = new GooificationState
                    {
                        Position = pawn.PositionHeld,
                        Map = pawn.MapHeld,
                        Drops = new List<Thing>()
                    };
                    __state.Drops.AddRange(pawn.equipment?.AllEquipmentListForReading ?? new List<ThingWithComps>());
                    __state.Drops.AddRange(pawn.apparel?.WornApparel ?? new List<Apparel>());
                    __state.Drops.AddRange(pawn.inventory?.innerContainer.ToList() ?? new List<Thing>());
                }
            }
        }
    }

    public static void Postfix(Pawn __instance, GooificationState __state)
    {
        if (__state != null && __state.Map != null)
        {
            foreach (var drop in __state.Drops)
            {
                drop.holdingOwner?.Remove(drop);
                if (drop.Spawned)
                {
                    drop.DeSpawn();
                }
                var old = drop.def.category;
                drop.def.category = ThingCategory.Mote;
                GenSpawn.Spawn(drop, __state.Position, __state.Map);
                drop.SetForbidden(true);
                drop.def.category = old;
            }
            __instance.Corpse?.Destroy();
        }
    }
}
