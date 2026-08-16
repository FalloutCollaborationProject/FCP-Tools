using Verse.AI;

namespace Thek_BuildingArrivalMode
{
    public class JobGiver_BuildingArrivalMode_Kidnap : JobGiver_Kidnap
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (KidnapAIUtility.TryFindGoodKidnapVictim(pawn, 18f, out var victim) && !GenAI.InDangerousCombat(pawn))
            {
                Job job = JobMaker.MakeJob(JobDefOfs.Thek_BuildingArrivalMode_Kidnap);
                job.targetA = PawnsArrivalModeWorker_BuildingArrivalMode.modExtension.tileToSpawn; 
                job.targetB = victim;
                job.count = 1;
                return job;
            }
            return null;
        }
    }
}
