namespace FCP.Core.PowerArmor;

// TODO look at moving this somewhere else
public class CompProperties_ApparelRequirement : CompProperties
{
    public List<ThingDef> requiredApparels;
    public TraitDef requiredTrait;

    public CompProperties_ApparelRequirement() => compClass = typeof(CompApparelRequirement);
}

public class CompApparelRequirement : ThingComp
{
    public CompProperties_ApparelRequirement Props => base.props as CompProperties_ApparelRequirement;

    public bool HasRequiredApparel(Pawn pawn)
    {
        return Props.requiredApparels is null || pawn.apparel.WornApparel.Any(y => Props.requiredApparels.Contains(y.def));
    }

    public bool HasRequiredTrait(Pawn pawn)
    {
        return Props.requiredTrait is null || pawn.story.traits.GetTrait(Props.requiredTrait) != null;
    }

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

    public AcceptanceReport CanWear(Pawn pawn)
    {
        if (!HasRequiredTrait(pawn))
        {
            return "RR.RequiresTrait".Translate(Props.requiredTrait.degreeDatas[0].label);
        }
        if (!HasRequiredApparel(pawn))
        {
            if (Props.requiredApparels.Count == 1)
            {
                return "RR.RequiresApparel".Translate(Props.requiredApparels[0].label);
            }

            return "RR.RequiresApparelsAnyOf".Translate(string.Join(", ", Props.requiredApparels.Select(x => x.label)));
        }

        return true;
    }
}