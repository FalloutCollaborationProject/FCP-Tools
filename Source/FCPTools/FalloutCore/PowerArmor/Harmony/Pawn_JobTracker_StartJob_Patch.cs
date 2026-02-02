using HarmonyLib;
using Verse.AI;

namespace FCP.Core.PowerArmor;

[HarmonyPatch(typeof(Pawn_JobTracker), "StartJob")]
public static class Pawn_JobTracker_StartJob_Patch
{
    public static void Postfix(Pawn_JobTracker __instance, ref Job newJob, JobCondition lastJobEndCondition, ref Pawn ___pawn)
    {
        Pawn pawn = ___pawn;
        if (pawn?.apparel?.WornApparel == null) 
            return;

        if (newJob.def != JobDefOf.LayDown) 
            return;
        
        foreach (Apparel apparel in pawn.apparel.WornApparel)
        {
            var props = apparel.def.GetCompProperties<CompProperties_PowerArmor>();
            if (props != null && !props.canSleep)
            {
                pawn.jobs.StartJob(JobMaker.MakeJob(JobDefOf.RemoveApparel, apparel), resumeCurJobAfterwards: true);
            }
        }
    }
}