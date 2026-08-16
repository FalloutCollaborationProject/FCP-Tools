namespace FCP.Ranks.Jobs;

[DefOf]
public static class RankJobDefOf
{
    public static JobDef FCP_Job_RankTraining;
    public static JobDef FCP_Job_RankDrilling;
    public static JobDef FCP_Job_RankDrillingPartner;
    public static JobDef FCP_Job_RankStudying;
    public static JobDef FCP_Job_RankStudyingPartner;
    public static JobDef FCP_Job_VisitMentor;
    public static JobDef FCP_Job_RankGearMaintenance;
    public static JobDef FCP_Job_RankInspection;
    public static JobDef FCP_Job_RankInspectionPartner;
    public static JobDef FCP_Job_PunishmentDetail;
    public static JobDef FCP_Job_FormUp;

    static RankJobDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(RankJobDefOf));
    }
}
