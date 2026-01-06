namespace RangerRick_PowerArmor
{
    public class CompProperties_ApparelDependency : CompProperties
    {
        public CompProperties_ApparelDependency() => compClass = typeof(CompApparelDependency);
    }

    public class CompApparelDependency : ThingComp
    {
        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);
            foreach (var apparel in pawn.apparel.WornApparel.ToList())
            {
                var comp = apparel.GetComp<CompApparelRequirement>();
                if (comp != null && comp.Props.requiredApparels != null && comp.HasRequiredApparel(pawn) is false)
                {
                    pawn.apparel.TryDrop(apparel);
                }
            }
        }
    }
}
