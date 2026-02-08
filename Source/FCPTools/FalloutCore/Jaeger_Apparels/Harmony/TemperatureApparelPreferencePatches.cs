using HarmonyLib;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace FCP.Core;

[HarmonyPatch]
public static class TemperatureApparelPreferencePatches
{

    private sealed class PawnGenState
    {
        public PawnGenerationRequest request;
        public bool isNew;
        public bool completed;
        public int markedTick;
    }

    private static readonly ConditionalWeakTable<Pawn, PawnGenState> stateByPawn =
        new ConditionalWeakTable<Pawn, PawnGenState>();

    [ThreadStatic] private static GenerationContext tlsContext;

    private sealed class GenerationContext
    {
        public HashSet<ThingDef> avoided;
        public HashSet<ThingDef> incompatible;
        public HashSet<ThingDef> forced;
    }

    private const float ForcedCommonalityMultiplier = 10000f;

    private static bool cachesBuilt;
    private static readonly object cacheLock = new object();

    private static List<ThingDef> cachedPrefApparelDefs;

    private static readonly Dictionary<ThingDef, CompProperties_TemperatureApparelPreference> cachedPropsByDef =
        new Dictionary<ThingDef, CompProperties_TemperatureApparelPreference>();

    private static readonly Dictionary<BodyDef, Dictionary<ThingDef, HashSet<ThingDef>>> incompatibleByBody =
        new Dictionary<BodyDef, Dictionary<ThingDef, HashSet<ThingDef>>>();

    private static void EnsureCachesBuilt()
    {
        if (cachesBuilt) return;

        lock (cacheLock)
        {
            if (cachesBuilt) return;

            cachedPrefApparelDefs = new List<ThingDef>();

            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                if (def == null || !def.IsApparel) continue;

                CompProperties_TemperatureApparelPreference p =
                    def.GetCompProperties<CompProperties_TemperatureApparelPreference>();

                if (p == null || !p.enabled) continue;

                cachedPrefApparelDefs.Add(def);
                cachedPropsByDef[def] = p;
            }

            cachesBuilt = true;
        }
    }

    private static Dictionary<ThingDef, HashSet<ThingDef>> GetOrBuildIncompatibilityMap(BodyDef body)
    {
        if (body == null) return null;

        if (incompatibleByBody.TryGetValue(body, out var map))
            return map;

        EnsureCachesBuilt();

        map = new Dictionary<ThingDef, HashSet<ThingDef>>();

        var allApparel = DefDatabase<ThingDef>.AllDefsListForReading
            .Where(d => d != null && d.IsApparel && d.apparel != null)
            .ToList();

        for (int i = 0; i < allApparel.Count; i++)
        {
            ThingDef a = allApparel[i];
            HashSet<ThingDef> incompatible = null;

            for (int j = 0; j < allApparel.Count; j++)
            {
                if (i == j) continue;

                ThingDef b = allApparel[j];

                if (!ApparelUtility.CanWearTogether(a, b, body))
                {
                    if (incompatible == null)
                        incompatible = new HashSet<ThingDef>();
                    incompatible.Add(b);
                }
            }

            if (incompatible != null)
                map[a] = incompatible;
        }

        incompatibleByBody[body] = map;
        return map;
    }

    [HarmonyPatchCategory(FCPCoreMod.LatePatchesCategory)]
    [HarmonyPatch(typeof(PawnGenerator), nameof(PawnGenerator.GeneratePawn),
        new Type[] { typeof(PawnGenerationRequest) })]
    public static class Patch_PawnGenerator_GeneratePawn
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn __result, PawnGenerationRequest request)
        {
            if (__result == null) return;

            PawnGenState state = stateByPawn.GetOrCreateValue(__result);
            state.request = request;
            state.isNew = true;
            state.completed = false;
            state.markedTick = Find.TickManager.TicksGame;

            if (FCPLog.VerboseEnabled)
                FCPLog.Verbose($"MARK pawn={SafePawnLabel(__result)} kind={request.KindDef?.defName ?? "null"} faction={request.Faction?.Name ?? "null"} ctx={request.Context} tile={request.Tile}");
        }
    }

    [HarmonyPatchCategory(FCPCoreMod.LatePatchesCategory)]
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.SpawnSetup))]
    public static class Patch_Pawn_SpawnSetup
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn __instance, Map map, bool respawningAfterLoad)
        {
            if (respawningAfterLoad)
            {
                if (FCPLog.VerboseEnabled)
                    FCPLog.Verbose($"SpawnSetup skip pawn={SafePawnLabel(__instance)} reason=respawnAfterLoad");
                return;
            }

            if (FCPLog.VerboseEnabled)
                FCPLog.Verbose($"SpawnSetup enter pawn={SafePawnLabel(__instance)} map={map?.Index.ToString() ?? "null"} spawned={__instance.Spawned}");

            ApplyGenerationOverrideIfEligible(__instance, map);
        }
    }

    [HarmonyPatchCategory(FCPCoreMod.LatePatchesCategory)]
    [HarmonyPatch(typeof(PawnApparelGenerator), "CanUsePair")]
    public static class Patch_PawnApparelGenerator_CanUsePair
    {
        [HarmonyPrefix]
        public static bool Prefix(ThingStuffPair pair, ref bool __result)
        {
            GenerationContext ctx = tlsContext;
            if (ctx == null)
                return true;

            ThingDef def = pair.thing;
            if (def == null)
                return true;

            if (ctx.avoided != null && ctx.avoided.Contains(def))
            {
                __result = false;
                return false;
            }

            if (ctx.incompatible != null && ctx.incompatible.Contains(def))
            {
                __result = false;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatchCategory(FCPCoreMod.LatePatchesCategory)]
    [HarmonyPatch(typeof(ThingStuffPair), "get_Commonality")]
    public static class Patch_ThingStuffPair_Commonality
    {
        [HarmonyPostfix]
        public static void Postfix(ThingStuffPair __instance, ref float __result)
        {
            GenerationContext ctx = tlsContext;
            if (ctx == null || __result <= 0f)
                return;

            ThingDef def = __instance.thing;
            if (def == null)
                return;

            if (ctx.avoided != null && ctx.avoided.Contains(def))
            {
                __result = 0f;
                return;
            }

            if (ctx.incompatible != null && ctx.incompatible.Contains(def))
            {
                __result = 0f;
                return;
            }

            if (ctx.forced != null && ctx.forced.Contains(def))
            {
                __result *= ForcedCommonalityMultiplier;
            }
        }
    }

    private static void ApplyGenerationOverrideIfEligible(Pawn pawn, Map map)
    {
        if (pawn == null || pawn.RaceProps == null || !pawn.RaceProps.Humanlike)
        {
            if (FCPLog.VerboseEnabled)
                FCPLog.Verbose($"Apply skip pawn={SafePawnLabel(pawn)} reason=notHumanlike");
            return;
        }

        if (!stateByPawn.TryGetValue(pawn, out PawnGenState state))
        {
            if (FCPLog.VerboseEnabled)
                FCPLog.Verbose($"Apply skip pawn={SafePawnLabel(pawn)} reason=noState");
            return;
        }

        if (!state.isNew)
        {
            if (FCPLog.VerboseEnabled)
                FCPLog.Verbose($"Apply skip pawn={SafePawnLabel(pawn)} reason=notMarkedNew");
            return;
        }

        if (state.completed)
        {
            if (FCPLog.VerboseEnabled)
                FCPLog.Verbose($"Apply skip pawn={SafePawnLabel(pawn)} reason=alreadyCompleted");
            return;
        }

        if (state.request.Context == PawnGenerationContext.PlayerStarter)
        {
            FCPLog.Verbose("Apply skip pawn=" + SafePawnLabel(pawn) + " reason=playerStarter");
            state.completed = true;
            state.isNew = false;
            return;
        }

        float tempC = map != null && map.mapTemperature != null ? map.mapTemperature.OutdoorTemp : float.NaN;
        if (float.IsNaN(tempC))
        {
            if (FCPLog.VerboseEnabled)
                FCPLog.Verbose($"Apply skip pawn={SafePawnLabel(pawn)} reason=noTemp map={map != null} mapTemp={map != null && map.mapTemperature != null}");
            return;
        }

        EnsureCachesBuilt();

        bool verbose = FCPLog.VerboseEnabled;
        Stopwatch sw = null;
        if (verbose)
            sw = Stopwatch.StartNew();

        try
        {
            HashSet<ThingDef> protectedDefs = BuildProtectedApparelSet(pawn);
            HashSet<ThingDef> avoidedNow = BuildAvoidedNow(tempC, pawn, protectedDefs);
            HashSet<ThingDef> forcedNow = BuildForcedNow(tempC, pawn, protectedDefs, avoidedNow);
            HashSet<ThingDef> incompatibleNow = BuildIncompatibleWithForced(pawn, protectedDefs, avoidedNow, forcedNow);

            if (verbose)
            {
                FCPLog.Verbose($"Apply RUN pawn={SafePawnLabel(pawn)} temp={tempC:F1} protected={protectedDefs.Count} avoided={avoidedNow.Count} forced={forcedNow.Count} incompatible={incompatibleNow.Count} ctx={state.request.Context} tile={state.request.Tile}");

                if (forcedNow.Count > 0)
                    FCPLog.Verbose($"Apply forced sample pawn={SafePawnLabel(pawn)} forced={JoinDefNames(forcedNow, 6)}");

                FCPLog.Verbose($"Apply worn BEFORE pawn={SafePawnLabel(pawn)} worn={DescribeWornApparel(pawn)}");
            }

            int removedCount;
            int lockedSkippedCount;
            RemoveAllApparelExceptLocked(pawn, out removedCount, out lockedSkippedCount);

            if (verbose)
                FCPLog.Verbose($"Apply strip pawn={SafePawnLabel(pawn)} removed={removedCount} lockedSkipped={lockedSkippedCount} wornAfterStrip={DescribeWornApparel(pawn)}");

            var ctx = new GenerationContext
            {
                avoided = avoidedNow,
                forced = forcedNow,
                incompatible = incompatibleNow
            };

            try
            {
                tlsContext = ctx;
                PawnApparelGenerator.GenerateStartingApparelFor(pawn, state.request);
            }
            finally
            {
                tlsContext = null;
            }

            if (verbose)
            {
                FCPLog.Verbose($"Apply worn AFTER pawn={SafePawnLabel(pawn)} worn={DescribeWornApparel(pawn)}");

                if (forcedNow.Count > 0)
                {
                    bool anyForcedWorn = IsAnyForcedWorn(pawn, forcedNow);
                    if (!anyForcedWorn)
                    {
                        FCPLog.Verbose($"Apply WARNING pawn={SafePawnLabel(pawn)} forcedCount={forcedNow.Count} but none worn. pawnKind={pawn.kindDef?.defName ?? "null"} pawnKindApparelTags={DescribeKindApparelTags(pawn.kindDef)}");
                        FCPLog.Verbose($"Apply WARNING forced def details={DescribeForcedDefs(forcedNow)}");
                    }
                    else
                    {
                        FCPLog.Verbose($"Apply forced OK pawn={SafePawnLabel(pawn)} forcedWorn=1");
                    }
                }
            }

            state.completed = true;
            state.isNew = false;
        }
        finally
        {
            if (sw != null)
            {
                sw.Stop();
                FCPLog.Verbose($"Apply time pawn={SafePawnLabel(pawn)} ms={sw.ElapsedMilliseconds} markedTick={state.markedTick}");
            }
        }
    }

    private static HashSet<ThingDef> BuildAvoidedNow(float tempC, Pawn pawn, HashSet<ThingDef> protectedDefs)
    {
        var set = new HashSet<ThingDef>();

        for (int i = 0; i < cachedPrefApparelDefs.Count; i++)
        {
            ThingDef def = cachedPrefApparelDefs[i];
            if (protectedDefs.Contains(def)) continue;
            if (!def.apparel.PawnCanWear(pawn)) continue;

            CompProperties_TemperatureApparelPreference p = cachedPropsByDef[def];
            if (ShouldAvoid(p, tempC))
                set.Add(def);
        }

        return set;
    }

    private static HashSet<ThingDef> BuildForcedNow(float tempC, Pawn pawn, HashSet<ThingDef> protectedDefs, HashSet<ThingDef> avoided)
    {
        var set = new HashSet<ThingDef>();

        for (int i = 0; i < cachedPrefApparelDefs.Count; i++)
        {
            ThingDef def = cachedPrefApparelDefs[i];
            if (protectedDefs.Contains(def)) continue;
            if (avoided.Contains(def)) continue;
            if (!def.apparel.PawnCanWear(pawn)) continue;

            CompProperties_TemperatureApparelPreference p = cachedPropsByDef[def];

            if (
                (!float.IsNegativeInfinity(p.forceBelowTempC) && tempC < p.forceBelowTempC) ||
                (!float.IsPositiveInfinity(p.forceAboveTempC) && tempC > p.forceAboveTempC)
            )
            {
                set.Add(def);
            }
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

        if (forced.Count == 0)
            return set;

        var map = GetOrBuildIncompatibilityMap(pawn.RaceProps.body);
        if (map == null)
            return set;

        foreach (ThingDef f in forced)
        {
            if (!map.TryGetValue(f, out var incompatible)) continue;

            foreach (ThingDef cand in incompatible)
            {
                if (protectedDefs.Contains(cand)) continue;
                if (avoided.Contains(cand)) continue;
                if (forced.Contains(cand)) continue;
                set.Add(cand);
            }
        }

        return set;
    }

    private static bool ShouldAvoid(CompProperties_TemperatureApparelPreference p, float t)
    {
        if (!float.IsPositiveInfinity(p.avoidAboveTempC) && t > p.avoidAboveTempC) return true;
        if (!float.IsNegativeInfinity(p.avoidBelowTempC) && t < p.avoidBelowTempC) return true;
        return false;
    }

    private static HashSet<ThingDef> BuildProtectedApparelSet(Pawn pawn)
    {
        var set = new HashSet<ThingDef>();

        if (pawn.kindDef != null && pawn.kindDef.apparelRequired != null)
        {
            for (int i = 0; i < pawn.kindDef.apparelRequired.Count; i++)
            {
                ThingDef d = pawn.kindDef.apparelRequired[i];
                if (d != null) set.Add(d);
            }
        }

        if (pawn.kindDef != null && pawn.kindDef.specificApparelRequirements != null)
        {
            for (int i = 0; i < pawn.kindDef.specificApparelRequirements.Count; i++)
            {
                var r = pawn.kindDef.specificApparelRequirements[i];
                if (r != null && r.ApparelDef != null)
                    set.Add(r.ApparelDef);
            }
        }

        if (pawn.apparel != null)
        {
            foreach (Apparel a in pawn.apparel.WornApparel)
            {
                if (a != null && pawn.apparel.IsLocked(a))
                    set.Add(a.def);
            }
        }

        return set;
    }

    private static void RemoveAllApparelExceptLocked(Pawn pawn, out int removedCount, out int lockedSkippedCount)
    {
        removedCount = 0;
        lockedSkippedCount = 0;

        if (pawn == null || pawn.apparel == null)
            return;

        var worn = pawn.apparel.WornApparel;
        for (int i = worn.Count - 1; i >= 0; i--)
        {
            Apparel a = worn[i];
            if (a == null) continue;

            if (pawn.apparel.IsLocked(a))
            {
                lockedSkippedCount++;
                continue;
            }

            pawn.apparel.Remove(a);
            removedCount++;
        }
    }

    private static bool IsAnyForcedWorn(Pawn pawn, HashSet<ThingDef> forced)
    {
        if (pawn == null || pawn.apparel == null || forced == null || forced.Count == 0)
            return false;

        var worn = pawn.apparel.WornApparel;
        for (int i = 0; i < worn.Count; i++)
        {
            Apparel a = worn[i];
            if (a == null) continue;
            if (forced.Contains(a.def))
                return true;
        }

        return false;
    }

    private static string DescribeWornApparel(Pawn pawn)
    {
        if (pawn == null || pawn.apparel == null)
            return "(none)";

        var worn = pawn.apparel.WornApparel;
        if (worn == null || worn.Count == 0)
            return "(none)";

        var sb = new StringBuilder();
        for (int i = 0; i < worn.Count; i++)
        {
            Apparel a = worn[i];
            if (a == null) continue;

            if (sb.Length > 0) sb.Append(" | ");

            bool locked = pawn.apparel.IsLocked(a);
            sb.Append(a.def != null ? a.def.defName : "null");
            sb.Append(locked ? "[L]" : "");
        }

        return sb.ToString();
    }

    private static string DescribeKindApparelTags(PawnKindDef kind)
    {
        if (kind == null)
            return "(null)";

        if (kind.apparelTags == null || kind.apparelTags.Count == 0)
            return "(none)";

        return string.Join(",", kind.apparelTags);
    }

    private static string DescribeForcedDefs(HashSet<ThingDef> forced)
    {
        if (forced == null || forced.Count == 0)
            return "(none)";

        int n = 0;
        var sb = new StringBuilder();
        foreach (ThingDef d in forced)
        {
            if (d == null) continue;
            if (n++ > 0) sb.Append(" || ");
            sb.Append(d.defName);

            sb.Append(" tags=");
            if (d.apparel != null && d.apparel.tags != null && d.apparel.tags.Count > 0)
                sb.Append(string.Join(",", d.apparel.tags));
            else
                sb.Append("(none)");

            sb.Append(" layers=");
            if (d.apparel != null && d.apparel.layers != null && d.apparel.layers.Count > 0)
                sb.Append(string.Join(",", d.apparel.layers.Select(l => l != null ? l.defName : "null")));
            else
                sb.Append("(none)");

            if (n >= 8) break;
        }

        return sb.ToString();
    }

    private static string JoinDefNames(IEnumerable<ThingDef> defs, int max)
    {
        if (defs == null) return "(none)";
        int n = 0;
        var sb = new StringBuilder();
        foreach (ThingDef d in defs)
        {
            if (d == null) continue;
            if (n++ > 0) sb.Append(",");
            sb.Append(d.defName);
            if (n >= max) break;
        }
        if (n == 0) return "(none)";
        return sb.ToString();
    }

    private static string SafePawnLabel(Pawn pawn)
    {
        if (pawn == null) return "null";
        try
        {
            return pawn.LabelShortCap + "|" + (pawn.Faction != null ? pawn.Faction.Name : "null") + "|" + pawn.thingIDNumber;
        }
        catch
        {
            return pawn.ToStringSafe();
        }
    }

}