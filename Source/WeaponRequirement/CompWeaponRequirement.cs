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

public abstract class WeaponRequirement
{
    public virtual bool RequiresCheckingOnTick => false;
    public abstract bool RequirementMet(Pawn pawn, Thing equipment, bool onTick = false);
}

public class WeaponRequirement_AnyInner : WeaponRequirement
{
    [UsedImplicitly] public List<WeaponRequirement> requirements;

    public override bool RequiresCheckingOnTick => requirements.Any(r => r.RequiresCheckingOnTick);

    public override bool RequirementMet(Pawn pawn, Thing equipment, bool onTick = false)
    {
        foreach (WeaponRequirement req in requirements)
        {
            if (onTick && !req.RequiresCheckingOnTick)
                continue;

            if (req.RequirementMet(pawn, equipment))
                return true;
        }

        return false;
    }
}

public class WeaponRequirement_AllInner : WeaponRequirement
{
    [UsedImplicitly] public List<WeaponRequirement> requirements;

    public override bool RequiresCheckingOnTick => requirements.Any(r => r.RequiresCheckingOnTick);

    public override bool RequirementMet(Pawn pawn, Thing equipment, bool onTick = false)
    {
        foreach (WeaponRequirement req in requirements)
        {
            if (onTick && !req.RequiresCheckingOnTick)
                continue;

            if (!req.RequirementMet(pawn, equipment))
                return false;
        }

        return true;
    }
}

public class WeaponRequirement_Xenotype : WeaponRequirement
{
    [UsedImplicitly] public HashSet<XenotypeDef> allowedXenotypes = [];
    [UsedImplicitly] public HashSet<XenotypeDef> bannedXenotypes = [];

    public override bool RequirementMet(Pawn pawn, Thing equipment, bool onTick = false)
    {
        XenotypeDef xenotype = pawn.genes?.Xenotype;

        if (xenotype is null)
            return false;

        if (bannedXenotypes.Contains(xenotype))
            return false;

        if (!allowedXenotypes.Contains(xenotype))
            return false;

        return true;
    }
}

public class WeaponRequirement_AnyApparel : WeaponRequirement
{
    [UsedImplicitly] public HashSet<ThingDef> requiredApparel = [];

    public override bool RequirementMet(Pawn pawn, Thing equipment, bool onTick = false)
    {
        // Skip and return true if there's not any in the list, most likely because they were all mayrequired.
        if (!requiredApparel.Any())
            return true;
        
        foreach (Apparel apparel in pawn.apparel.WornApparel)
        {
            if (requiredApparel.Contains(apparel.def))
                return true;
        }

        return false;
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