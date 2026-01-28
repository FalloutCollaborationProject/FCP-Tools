namespace FCP.WeaponRequirement;

public class WeaponRequirement_Xenotype : WeaponRequirement
{
    [UsedImplicitly] public List<XenotypeDef> allowedXenotypes = [];
    [UsedImplicitly] public List<XenotypeDef> bannedXenotypes = [];

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

    public override string RejectionReason(Pawn pawn, Thing equipment)
    {
        XenotypeDef xenotype = pawn.genes?.Xenotype;

        if (xenotype is null)
            return "FCP_WeaponReq_NoXenotype".Translate();

        if (bannedXenotypes.Contains(xenotype))
            return "FCP_WeaponReq_BannedXenotype".Translate(xenotype.label);

        if (allowedXenotypes.Any() && !allowedXenotypes.Contains(xenotype))
            return "FCP_WeaponReq_WrongXenotype".Translate(
                string.Join(", ", allowedXenotypes.Select(x => x.label)));

        return null;
    }
}