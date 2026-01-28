namespace FCP.WeaponRequirement;

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