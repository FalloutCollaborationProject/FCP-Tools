using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace FCP.Core.Robotics
{
    public class WildRobotSpawnExtension : DefModExtension
    {
        public float hostileChance = 0.1f;
    }

    [HarmonyPatch(typeof(PawnGenerator), nameof(PawnGenerator.GeneratePawn), new[] { typeof(PawnKindDef), typeof(Faction), typeof(PlanetTile?) })]
    public static class WildRobotSpawn_Patch
    {
        [ThreadStatic]
        internal static bool Suppress;

        public static void Postfix(PawnKindDef kindDef, Faction faction, ref Pawn __result)
        {
            if (faction != null || __result == null || Suppress)
            {
                return;
            }

            WildRobotSpawnExtension ext = kindDef.GetModExtension<WildRobotSpawnExtension>();
            if (ext == null)
            {
                return;
            }

            CompRefuelable fuel = __result.GetComp<CompRefuelable>();
            if (fuel != null && fuel.Fuel > 0f)
            {
                fuel.ConsumeFuel(fuel.Fuel * Rand.Range(0f, 1f));
            }

            if (__result.Faction != Faction.OfMechanoids && Rand.Chance(ext.hostileChance))
            {
                __result.SetFaction(Faction.OfMechanoids);
            }
        }
    }
}
