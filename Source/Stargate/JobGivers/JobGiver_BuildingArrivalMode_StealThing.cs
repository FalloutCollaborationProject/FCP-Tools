using UnityEngine;
using Verse.AI;

namespace Thek_BuildingArrivalMode
{
    public class JobGiver_BuildingArrivalMode_StealThing : JobGiver_Steal
    {
        //Mostly copied from JobGiver_Kidnap, with a few changes
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (StealAIUtility.TryFindBestItemToSteal(pawn.Position, pawn.Map, 12f, out var item, pawn) && !GenAI.InDangerousCombat(pawn))
            {
                Job job = JobMaker.MakeJob(JobDefOfs.Thek_BuildingArrivalMode_StealThing);
                job.targetA = PawnsArrivalModeWorker_BuildingArrivalMode.modExtension.tileToSpawn; //The place where they have spawned from
                job.targetB = item;
                job.count = Mathf.Min(item.stackCount, (int)(pawn.GetStatValue(StatDefOf.CarryingCapacity) / item.def.VolumePerUnit));
                return job;
            }
            return null;
        }
    }
}
