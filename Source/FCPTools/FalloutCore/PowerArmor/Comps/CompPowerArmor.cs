using FCP.Core.Access;

namespace FCP.Core.PowerArmor;

public class CompProperties_PowerArmor : CompProperties
{
    public HediffDef hediffOnEmptyFuel;
    public List<WorkTypeDef> workDisables;
    public bool canSleep = true;
    public bool ignoresLegs = false;
    public CompProperties_PowerArmor()
    {
        this.compClass = typeof(CompPowerArmor);
    }
}

[HotSwappable]
public class CompPowerArmor : ThingComp
{
    public CompProperties_PowerArmor Props => base.props as CompProperties_PowerArmor;

    private CompRefuelable _compRefuelable;
    public CompRefuelable CompRefuelable => _compRefuelable ??= parent.GetComp<CompRefuelable>();

    public override void CompTick()
    {
        base.CompTick();
        if (parent is not Apparel apparel || apparel.Wearer is null) 
            return;
        
        if (CompRefuelable is not CompRefuelable comp) 
            return;
                
        comp.ConsumeFuel(comp.P_ConsumptionRatePerTick);
        if (Props.hediffOnEmptyFuel == null || comp.HasFuel) 
            return;
        
        Hediff hediff = apparel.Wearer.health.hediffSet.GetFirstHediffOfDef(Props.hediffOnEmptyFuel);
        if (hediff is null)
        {
            apparel.Wearer.health.AddHediff(Props.hediffOnEmptyFuel);
        }
    }

    public override void Notify_Unequipped(Pawn pawn)
    {
        base.Notify_Unequipped(pawn);
        if (Props.hediffOnEmptyFuel == null) 
            return;
        
        Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediffOnEmptyFuel);
        if (hediff != null)
        {
            pawn.health.RemoveHediff(hediff);
        }
    }

    public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
    {
        if (parent is not Apparel apparel) 
            yield break;
        
        var comp = parent.GetComp<CompRefuelable>();
        if (comp.Props.showFuelGizmo && Find.Selector.SingleSelectedThing == apparel.Wearer)
        {
            var gizmoRefuelableFuelStatus = new Gizmo_SetFuelLevel(comp);
            yield return gizmoRefuelableFuelStatus;
        }
        foreach (Gizmo gizmo in comp.CompGetGizmosExtra())
        {
            yield return gizmo;
        }
    }
}