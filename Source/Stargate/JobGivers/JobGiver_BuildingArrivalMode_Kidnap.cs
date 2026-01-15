using Verse.AI;

namespace Thek_BuildingArrivalMode
{
    public class JobGiver_BuildingArrivalMode_Kidnap : JobGiver_Kidnap
    {
    //Mostly copied from JobGiver_Kidnap, with a few changes
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (KidnapAIUtility.TryFindGoodKidnapVictim(pawn, 18f, out var victim) && !GenAI.InDangerousCombat(pawn))
            {
                Job job = JobMaker.MakeJob(JobDefOfs.Thek_BuildingArrivalMode_Kidnap);
                job.targetA = PawnsArrivalModeWorker_BuildingArrivalMode.modExtension.tileToSpawn; //The place where they have spawned from
                job.targetB = victim;
                job.count = 1;
                return job;
            }
            return null;
        }
    }
}
