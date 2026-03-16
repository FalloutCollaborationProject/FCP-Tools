namespace FCP.Core;

public class CompMechDetonator : ThingComp
{
    public CompProperties_MechDetonator Props => (CompProperties_MechDetonator)props;

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        if (parent.Faction != Faction.OfPlayer) 
            yield break;
        
        Gizmo detonateAction = new Command_Action
        {
            icon = Props.GetUiIcon(),
            defaultLabel = "FCP_MechDetonate".Translate(),
            action = () => { parent.Kill(); }
        };
        yield return detonateAction;

    }
}