namespace FCP.Core;

[UsedImplicitly]
public class CharacterTraitsDefinition : CharacterBaseDefinition
{
    public Dictionary<TraitDef, int> traits = new Dictionary<TraitDef, int>();

    public override bool AppliesPreGeneration => false;
    public override bool AppliesPostGeneration => !traits.NullOrEmpty();

    public override void ApplyToPawn(Pawn pawn)
    {
        if (pawn.story?.traits == null)
        {
            return;
        }

        foreach (KeyValuePair<TraitDef, int> trait in traits)
        {
            if (pawn.story.traits.HasTrait(trait.Key))
            {
                continue;
            }

            pawn.story.traits.GainTrait(new Trait(trait.Key, trait.Value));
        }
    }
}
