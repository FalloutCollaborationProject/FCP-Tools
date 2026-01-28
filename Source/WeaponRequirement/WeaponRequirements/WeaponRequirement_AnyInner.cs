namespace FCP.WeaponRequirement;

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

    public override string RejectionReason(Pawn pawn, Thing equipment)
    {
        // All inner requirements failed - return the first rejection reason
        foreach (WeaponRequirement req in requirements)
        {
            string reason = req.RejectionReason(pawn, equipment);
            if (reason != null)
                return reason;
        }

        return null;
    }
}