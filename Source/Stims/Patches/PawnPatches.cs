using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using StimPacks.Comps;
using StimPacks.DefExtensions;
using StimPacks.ModConfig;
using Verse;
using Verse.AI;

namespace StimPacks.Patches
{
    public static class PawnPatches
    {
        [HarmonyPatch(typeof(Pawn), nameof(Pawn.PostApplyDamage))]
        public static class TakeDamagePatch
        {
            [HarmonyPostfix]
            public static void Postfix(Pawn __instance, float totalDamageDealt)
            {
                if(!ConfigUI.Config.AutoStim)
                    return;
                if(__instance.story == null)
                    return;
                if(!ConfigUI.Config.TeetotalerAutoStim && __instance.story.traits.DegreeOfTrait(TraitDefOf.DrugDesire) < 0)
                    return;
                if (totalDamageDealt > 0)
                {
                    var stimpack = __instance.inventory.innerContainer
                        .FirstOrDefault(x => x.def.HasModExtension<IngestibleStimExtension>());
                    if (stimpack != null)
                    {
                        StimPack existing;
                        foreach (var hediff in __instance.health.hediffSet.hediffs)
                        {
                            existing = hediff.TryGetComp<StimPack>();
                            if (existing != null)
                            {
                                return;
                            }
                        }
                        Job job = JobMaker.MakeJob(JobDefOf.Ingest, stimpack);
                        job.count = 1;
                        __instance.jobs.TryTakeOrderedJob(job);
                    }
                }
            }
        }
    }
}