namespace FCP.WeaponRequirement;

public class WeaponRequirement_AnyApparel : WeaponRequirement
{
    [UsedImplicitly] public List<ThingDef> requiredApparel = [];

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