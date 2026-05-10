namespace FCP.Core;

public class WeaponRequirement_AnyApparel : WeaponRequirement
{
    [UsedImplicitly] public List<ThingDef> requiredApparel = [];

    public override bool RequirementMet(Pawn pawn, Thing equipment, bool onTick = false)
    {
        if (!requiredApparel.Any())
            return true;
        
        foreach (Apparel apparel in pawn.apparel.WornApparel)
        {
            if (requiredApparel.Contains(apparel.def))
                return true;
        }

        return false;
    }

    public override string RejectionReason(Pawn pawn, Thing equipment)
    {
        if (!requiredApparel.Any())
            return null;

        return "FCP_WeaponReq_RequiresApparel".Translate(
            string.Join(", ", requiredApparel.Select(x => x.label)));
    }
}
