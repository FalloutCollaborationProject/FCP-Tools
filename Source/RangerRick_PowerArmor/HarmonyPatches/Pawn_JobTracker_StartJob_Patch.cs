using HarmonyLib;
using Verse;
using Verse.AI;
using RimWorld;

namespace RangerRick_PowerArmor
{
    [HarmonyPatch(typeof(Pawn_JobTracker), "StartJob")]
    public static class Pawn_JobTracker_StartJob_Patch
    {
        public static void Postfix(Pawn_JobTracker __instance, ref Job newJob, JobCondition lastJobEndCondition)
        {
            var pawn = __instance.pawn;
            if (pawn?.apparel?.WornApparel == null) return;
            if (newJob.def == JobDefOf.LayDown)
            {
                foreach (var apparel in pawn.apparel.WornApparel)
                {
                    var props = apparel.def.GetCompProperties<CompProperties_PowerArmor>();
                    if (props != null && props.canSleep == false)
                    {
                        pawn.jobs.StartJob(JobMaker.MakeJob(JobDefOf.RemoveApparel, apparel), resumeCurJobAfterwards: true);
                    }
                }
            }
        }
    }
}