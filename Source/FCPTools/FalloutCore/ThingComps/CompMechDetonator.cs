namespace FCP.Core;

public class CompMechDetonator : ThingComp
{
    public CompProperties_MechDetonator Props => (CompProperties_MechDetonator)props;

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {

        if (parent.Faction == Faction.OfPlayer)
        {
            Command_Action command_Action = new Command_Action();
            command_Action.icon = Props.GetUiIcon();
            command_Action.defaultLabel = "FCP_MechDetonate".Translate();
            command_Action.action = delegate
            {
                //Log.Message("BOOM");
                parent.Kill();
            };
            yield return command_Action;
        }

    }
}