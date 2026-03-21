namespace FCP.Core.PowerArmor;

// TODO look at moving this somewhere else
[UsedImplicitly(ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.WithMembers)]
public class CompProperties_ApparelRequirement : CompProperties
{
    public List<ThingDef> requiredApparels;
    public TraitDef requiredTrait;

    public CompProperties_ApparelRequirement()
    {
        this.compClass = typeof(CompApparelRequirement);
    }
}

public class CompApparelRequirement : ThingComp
{
    private static class Keys
    {
        public const string RequiresTrait = "FCP_ApparelRequirement.RequiresTrait";
        public const string RequiresApparel = "FCP_ApparelRequirement.RequiresApparel";
        public const string AnyOfApparel = "FCP_ApparelRequirement.RequiresAnyOfApparel";
        public const string RequiresPowerArmorTraining = "FCP_ApparelRequirement.RequiresPowerArmorTraining";
    }
    
    public CompProperties_ApparelRequirement Props => base.props as CompProperties_ApparelRequirement;

    public bool HasRequiredApparel(Pawn pawn)
    {
        return Props.requiredApparels is null || pawn.apparel.WornApparel.Any(apparel => Props.requiredApparels.Contains(apparel.def));
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
            if (Props.requiredTrait == PowerArmorDefOf.FCP_Trait_Power_Armor_Trained)
                return Keys.RequiresPowerArmorTraining.Translate();
            
            return Keys.RequiresTrait.Translate(Props.requiredTrait.degreeDatas[0].label);
        }
        if (!HasRequiredApparel(pawn))
        {
            return Props.requiredApparels.Count == 1 
                ? Keys.RequiresApparel.Translate(Props.requiredApparels[0].label) 
                : Keys.AnyOfApparel.Translate(Props.requiredApparels.Select(def => def.label).ToCommaListOr());
        }

        return true;
    }
}