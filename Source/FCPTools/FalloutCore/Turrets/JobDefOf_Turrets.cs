namespace FCP.Core.Turrets
{
    [DefOf]
    public static class JobDefOf_Turrets
    {
        public static JobDef FCP_Job_InstallTurretWeapon;

        static JobDefOf_Turrets()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(JobDefOf_Turrets));
        }
    }
}
