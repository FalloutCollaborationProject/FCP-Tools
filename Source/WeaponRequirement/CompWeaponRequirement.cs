using FCP.Core;

namespace FCP.WeaponRequirement;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class CompProperties_WeaponRequirement : CompProperties
{
    public List<WeaponRequirement> requirements;

    public bool dontBlockEquip = false;
    public HediffDef requirementsNotMetHediff;

    public CompProperties_WeaponRequirement()
    {
        this.compClass = typeof(CompWeaponRequirement);
    }

    public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
    {
        if (requirements.NullOrEmpty())
            yield return "CompProperties_WeaponRequirement Has no Requirements";

        if (requirements.OfType<WeaponRequirement_AnyInner>().Any(x => !x.requirements.Any()))
            yield return "CompProperties_WeaponRequirement AnyInner has no Requirements";

        if (requirements.OfType<WeaponRequirement_AllInner>().Any(x => !x.requirements.Any()))
            yield return "CompProperties_WeaponRequirement AllInner has no Requirements";
    }
}

public class CompWeaponRequirement : ThingComp
{
    public const int TickInterval = GenTicks.TickLongInterval;

    public CompProperties_WeaponRequirement Props => base.props as CompProperties_WeaponRequirement;

    public bool RequirementsMet(Pawn pawn, bool onTick = false)
    {
        ThingWithComps equipment = parent;
        return Props.requirements.All(requirement => requirement.RequirementMet(pawn, equipment, onTick));
    }

    public void EquipmentTrackerTick()
    {
        if (Props.requirementsNotMetHediff == null)
            return;
        
        // Shouldn't be null since this is only called from equipment tracker
        var equipmentTracker = parent.ParentHolder as Pawn_EquipmentTracker;
        Pawn pawn = equipmentTracker?.pawn;
        Hediff hediff = pawn?.health.hediffSet.GetFirstHediffOfDef(Props.requirementsNotMetHediff);

        if (RequirementsMet(pawn, true))
        {
            if (hediff != null)
            {
                pawn.health.RemoveHediff(hediff);
            }
        }
        else if (hediff == null)
        {
            pawn?.health.AddHediff(Props.requirementsNotMetHediff);
        }
    }
}