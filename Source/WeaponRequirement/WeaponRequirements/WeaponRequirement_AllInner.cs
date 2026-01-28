namespace FCP.WeaponRequirement;

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

    public override string RejectionReason(Pawn pawn, Thing equipment)
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

        return null;
    }
}