namespace FCP.Core.PowerArmor;

public class CompProperties_ApparelDependency : CompProperties
{
    public CompProperties_ApparelDependency() => compClass = typeof(CompApparelDependency);
}

public class CompApparelDependency : ThingComp
{
    public override void Notify_Unequipped(Pawn pawn)
    {
        base.Notify_Unequipped(pawn);
        foreach (Apparel apparel in pawn.apparel.WornApparel.ToList())
        {
            if (!pawn.apparel.WornApparel.Contains(apparel))
                continue;

            var comp = apparel.GetComp<CompApparelRequirement>();
            if (comp?.Props.requiredApparels != null && !comp.HasRequiredApparel(pawn))
            {
                pawn.apparel.TryDrop(apparel);
            }
        }
    }
}