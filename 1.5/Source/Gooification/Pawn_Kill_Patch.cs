using RimWorld;
using Verse;
using System.Linq;
using System.Collections.Generic;
using HarmonyLib;

namespace FalloutCore
{
    [HarmonyPatch(typeof(Pawn), "Kill")]
    public static class Pawn_Kill_Patch
    {
        public static void Prefix(Pawn __instance, DamageInfo? dinfo, Hediff exactCulprit, out List<Thing> __state)
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
                        __state = new List<Thing>();
                        var pawn = __instance;
                        Thing thing = ThingMaker.MakeThing(ThingDef.Named(damage.FilthToSpawn));
                        GenSpawn.Spawn(thing, pawn.Position, pawn.Map, WipeMode.Vanish);
                        IntVec3 pos = pawn.PositionHeld;
                        __state.AddRange(pawn.equipment?.AllEquipmentListForReading ?? new List<ThingWithComps>());
                        __state.AddRange(pawn.apparel?.WornApparel ?? new List<Apparel>());
                        __state.AddRange(pawn.inventory?.innerContainer.ToList() ?? new List<Thing>());
                    }
                }
            }
        }

        public static void Postfix(Pawn __instance, List<Thing> __state)
        {
            if (__state != null)
            {
                var pos = __instance.Corpse.Position;
                var map = __instance.Corpse.Map;
                foreach (var drop in __state)
                {
                    drop.holdingOwner?.Remove(drop);
                    if (drop.Spawned)
                    {
                        drop.DeSpawn();
                    }
                    var old = drop.def.category;
                    drop.def.category = ThingCategory.Mote;
                    GenSpawn.Spawn(drop, pos, map);
                    drop.def.category = old;
                }
                __instance.Corpse.Destroy();
            }
        }
    }
}

