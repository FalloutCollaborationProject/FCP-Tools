namespace FCPVT;

public class CompProperties_RollingVaultDoorSingle : CompProperties
{
    public CompProperties_RollingVaultDoorSingle() => compClass = typeof(ThingComp_RollingVaultDoorSingle);
    public List<GraphicData_RollingVaultDoor> extraDoorGraphics = null;

    public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
    {
        foreach (string error in base.ConfigErrors(parentDef))
        {
            yield return error;
        }

        if (extraDoorGraphics == null)
        {
            yield return $"{VTLog.WarningMsgCol} [CompProperties_RollingVaultDoorSingle] No data found for <extraDoorGraphics>, please provide some.";
        }
    }
}
