namespace FCP.Core;

public class IncidentWorker_BountyHunters : IncidentWorker_RaidEnemy
{
    protected override bool CanFireNowSub(IncidentParms parms)
    {
        return NotorietyUtility.IsNotorious() && base.CanFireNowSub(parms);
    }
}
