using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace FCP.Core.TemperatureApparelPreference
{
    public static class TemperatureApparelPreferencePatches
    {
        private const int VerboseLogMax = 600;
        private static int verboseCount;

        private static readonly HashSet<int> newlyGeneratedPawnIds = new HashSet<int>();
        private static readonly HashSet<int> completedPawnIds = new HashSet<int>();

        private static readonly Dictionary<int, PawnGenerationRequest> requestByPawnId =
            new Dictionary<int, PawnGenerationRequest>();

        // ===== Generation overrides (active only while we call GenerateStartingApparelFor) =====
        private static bool generationOverrideActive;
        private static HashSet<ThingDef> generationAvoided;
        private static HashSet<ThingDef> generationIncompatible;
        private static HashSet<ThingDef> generationForced;

        // Big enough to dominate but still leaves vanilla randomness intact among non-forced items.
        private const float ForcedCommonalityMultiplier = 10000f;

        private static bool VerboseLoggingEnabled =>
            Mod.Instance?.Settings != null && Mod.Instance.Settings.verboseLogging;

        // ============================================================
        // Track newly generated pawns + cache PawnGenerationRequest
        // ============================================================
        [HarmonyPatch(typeof(PawnGenerator), nameof(PawnGenerator.GeneratePawn),
            new Type[] { typeof(PawnGenerationRequest) })]
        public static class Patch_PawnGenerator_GeneratePawn
        {
            [HarmonyPostfix]
            public static void Postfix(Pawn __result, PawnGenerationRequest request)
            {
                if (__result == null) return;

                int id = __result.thingIDNumber;
                if (id == 0) return;

                newlyGeneratedPawnIds.Add(id);
                requestByPawnId[id] = request;

                if (newlyGeneratedPawnIds.Count > 40000)
                    newlyGeneratedPawnIds.Clear();
                if (requestByPawnId.Count > 40000)
                    requestByPawnId.Clear();

                VLog($"MARK new pawn: {__result} id={id}");
            }
        }

        // ============================================================
        // Apply logic on SpawnSetup (new pawns only)
        // ============================================================
        [HarmonyPatch(typeof(Pawn), nameof(Pawn.SpawnSetup))]
        public static class Patch_Pawn_SpawnSetup
        {
            [HarmonyPostfix]
            public static void Postfix(Pawn __instance, Map map, bool respawningAfterLoad)
            {
                if (respawningAfterLoad) return;
                ApplyGenerationOverrideIfEligible(__instance, map);
            }
        }

        // ============================================================
        // HARD exclusion during generation: block avoided + incompatible defs at CanUsePair
        // ============================================================
        [HarmonyPatch(typeof(PawnApparelGenerator), "CanUsePair",
            new Type[] { typeof(ThingStuffPair), typeof(Pawn), typeof(float), typeof(bool), typeof(int) })]
        public static class Patch_PawnApparelGenerator_CanUsePair
        {
            [HarmonyPrefix]
            public static bool Prefix(ThingStuffPair pair, ref bool __result)
            {
                if (!generationOverrideActive)
                    return true;

                ThingDef def = pair.thing;
                if (def == null)
                    return true;

                if (generationAvoided != null && generationAvoided.Contains(def))
                {
                    __result = false;
                    return false;
                }

                if (generationIncompatible != null && generationIncompatible.Contains(def))
                {
                    __result = false;
                    return false;
                }

                return true;
            }
        }

        // ============================================================
        // Weight override:
        // - avoided/incompatible => commonality 0
        // - forced => massive multiplier
        // ============================================================
        [HarmonyPatch(typeof(ThingStuffPair), "get_Commonality")]
        public static class Patch_ThingStuffPair_Commonality
        {
            [HarmonyPostfix]
            public static void Postfix(ThingStuffPair __instance, ref float __result)
            {
                if (!generationOverrideActive || __result <= 0f)
                    return;

                ThingDef def = __instance.thing;
                if (def == null)
                    return;

                if (generationAvoided != null && generationAvoided.Contains(def))
                {
                    __result = 0f;
                    return;
                }

                if (generationIncompatible != null && generationIncompatible.Contains(def))
                {
                    __result = 0f;
                    return;
                }

                if (generationForced != null && generationForced.Contains(def))
                {
                    __result *= ForcedCommonalityMultiplier;
                    return;
                }
            }
        }

        // ============================================================
        // Core logic
        // ============================================================
        private static void ApplyGenerationOverrideIfEligible(Pawn pawn, Map map)
        {
            if (pawn?.RaceProps == null || !pawn.RaceProps.Humanlike)
                return;

            int id = pawn.thingIDNumber;
            if (!newlyGeneratedPawnIds.Contains(id) || completedPawnIds.Contains(id))
                return;

            float tempC = map?.mapTemperature?.OutdoorTemp ?? float.NaN;
            if (float.IsNaN(tempC))
                return;

            PawnGenerationRequest req;
            if (!requestByPawnId.TryGetValue(id, out req))
                return;

            HashSet<ThingDef> protectedDefs = BuildProtectedApparelSet(pawn);
            HashSet<ThingDef> avoidedNow = BuildAvoidedNow(tempC, pawn, protectedDefs);
            HashSet<ThingDef> forcedNow = BuildForcedNow(tempC, pawn, protectedDefs, avoidedNow);

            HashSet<ThingDef> incompatibleNow = BuildIncompatibleWithForced(pawn, protectedDefs, avoidedNow, forcedNow);

            VLog($"RUN: {pawn} temp={tempC:F1}C avoided={avoidedNow.Count} forced={forcedNow.Count} incompatible={incompatibleNow.Count}");

            RemoveAllApparelExceptLocked(pawn);

            try
            {
                generationAvoided = avoidedNow;
                generationForced = forcedNow;
                generationIncompatible = incompatibleNow;
                generationOverrideActive = true;

                PawnApparelGenerator.GenerateStartingApparelFor(pawn, req);
            }
            finally
            {
                generationOverrideActive = false;
                generationAvoided = null;
                generationForced = null;
                generationIncompatible = null;
            }

            completedPawnIds.Add(id);
            newlyGeneratedPawnIds.Remove(id);
            requestByPawnId.Remove(id);
        }

        // ============================================================
        // Set construction
        // ============================================================
        private static HashSet<ThingDef> BuildAvoidedNow(float tempC, Pawn pawn, HashSet<ThingDef> protectedDefs)
        {
            var set = new HashSet<ThingDef>();

            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs)
            {
                if (!def.IsApparel) continue;
                if (protectedDefs.Contains(def)) continue;
                if (def.apparel == null) continue;
                if (!def.apparel.PawnCanWear(pawn)) continue;

                var p = def.GetCompProperties<CompProperties_TemperatureApparelPreference>();
                if (p == null || !p.enabled) continue;

                if (ShouldAvoid(p, tempC))
                    set.Add(def);
            }

            return set;
        }

        private static HashSet<ThingDef> BuildForcedNow(float tempC, Pawn pawn, HashSet<ThingDef> protectedDefs, HashSet<ThingDef> avoided)
        {
            var set = new HashSet<ThingDef>();

            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs)
            {
                if (!def.IsApparel) continue;
                if (protectedDefs.Contains(def)) continue;
                if (avoided.Contains(def)) continue;
                if (def.apparel == null) continue;
                if (!def.apparel.PawnCanWear(pawn)) continue;

                var p = def.GetCompProperties<CompProperties_TemperatureApparelPreference>();
                if (p == null || !p.enabled) continue;

                bool force =
                    (!float.IsNegativeInfinity(p.forceBelowTempC) && tempC < p.forceBelowTempC) ||
                    (!float.IsPositiveInfinity(p.forceAboveTempC) && tempC > p.forceAboveTempC);

                if (force)
                    set.Add(def);
            }

            return set;
        }

        private static HashSet<ThingDef> BuildIncompatibleWithForced(
            Pawn pawn,
            HashSet<ThingDef> protectedDefs,
            HashSet<ThingDef> avoided,
            HashSet<ThingDef> forced)
        {
            var set = new HashSet<ThingDef>();

            if (forced == null || forced.Count == 0)
                return set;

            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs)
            {
                if (!def.IsApparel) continue;
                if (protectedDefs.Contains(def)) continue;
                if (avoided.Contains(def)) continue;
                if (forced.Contains(def)) continue;
                if (def.apparel == null) continue;
                if (!def.apparel.PawnCanWear(pawn)) continue;

                bool incompatible = false;

                foreach (ThingDef f in forced)
                {
                    // If either cannot be worn together, block the candidate.
                    if (!ApparelUtility.CanWearTogether(def, f, pawn.RaceProps.body))
                    {
                        incompatible = true;
                        break;
                    }
                }

                if (incompatible)
                    set.Add(def);
            }

            return set;
        }

        // ============================================================
        // Helpers
        // ============================================================
        private static bool ShouldAvoid(CompProperties_TemperatureApparelPreference p, float t)
        {
            if (!float.IsPositiveInfinity(p.avoidAboveTempC) && t > p.avoidAboveTempC) return true;
            if (!float.IsNegativeInfinity(p.avoidBelowTempC) && t < p.avoidBelowTempC) return true;
            return false;
        }

        private static HashSet<ThingDef> BuildProtectedApparelSet(Pawn pawn)
        {
            var set = new HashSet<ThingDef>();

            pawn.kindDef?.apparelRequired?.ForEach(d =>
            {
                if (d != null) set.Add(d);
            });

            pawn.kindDef?.specificApparelRequirements?.ForEach(r =>
            {
                if (r?.ApparelDef != null) set.Add(r.ApparelDef);
            });

            // Locked apparel are effectively protected.
            if (pawn.apparel != null)
            {
                var worn = pawn.apparel.WornApparel;
                for (int i = 0; i < worn.Count; i++)
                {
                    Apparel a = worn[i];
                    if (a != null && pawn.apparel.IsLocked(a))
                        set.Add(a.def);
                }
            }

            return set;
        }

        private static void RemoveAllApparelExceptLocked(Pawn pawn)
        {
            if (pawn?.apparel == null)
                return;

            var worn = pawn.apparel.WornApparel;
            for (int i = worn.Count - 1; i >= 0; i--)
            {
                Apparel a = worn[i];
                if (a == null) continue;

                if (pawn.apparel.IsLocked(a))
                    continue;

                pawn.apparel.Remove(a);
            }
        }

        private static void VLog(string msg)
        {
            if (!VerboseLoggingEnabled || verboseCount++ >= VerboseLogMax) return;
            Log.Message("[TemperatureApparelPreference] " + msg);
        }
    }
}
