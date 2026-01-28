using FCP.Core;

namespace FCP.WeaponRequirement;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class WeaponRequirementExtension : DefModExtension
{
    public List<WeaponRequirement> requirements;

    public bool dontBlockEquip;
    public HediffDef requirementsNotMetHediff;

    public override IEnumerable<string> ConfigErrors()
    {
        if (requirements.NullOrEmpty())
            yield return "WeaponRequirementExtension Has no Requirements";

        if (requirements.OfType<WeaponRequirement_AnyInner>().Any(x => !x.requirements.Any()))
            yield return "WeaponRequirementExtension AnyInner has no Requirements";

        if (requirements.OfType<WeaponRequirement_AllInner>().Any(x => !x.requirements.Any()))
            yield return "WeaponRequirementExtension AllInner has no Requirements";
    }

    public bool RequirementsMet(Pawn pawn, Thing equipment, bool onTick)
    {
        return requirements.All(requirement => requirement.RequirementMet(pawn, equipment, onTick));
    }

    public string RejectionReason(Pawn pawn, Thing equipment)
    {
        foreach (WeaponRequirement req in requirements)
        {
            if (!req.RequirementMet(pawn, equipment))
            {
                string reason = req.RejectionReason(pawn, equipment);
                if (reason != null)
                    return reason;
            }
        }

        return "FCP_WeaponReq_RequirementsNotMet".Translate();
    }

}

public static class WeaponRequirementUtility
{
    public const int TickInterval = GenTicks.TickLongInterval;

    public static void EquipmentTrackerTick(WeaponRequirementExtension ext, Pawn pawn, Thing equipment)
    {
        if (ext.requirementsNotMetHediff == null)
            return;
        
        // Shouldn't be null since this is only called from equipment tracker
        Hediff hediff = pawn?.health.hediffSet.GetFirstHediffOfDef(ext.requirementsNotMetHediff);

        if (ext.RequirementsMet(pawn, equipment, true))
        {
            if (hediff != null)
            {
                pawn.health.RemoveHediff(hediff);
            }
        }
        else if (hediff == null)
        {
            pawn?.health.AddHediff(ext.requirementsNotMetHediff);
        }
    }
}