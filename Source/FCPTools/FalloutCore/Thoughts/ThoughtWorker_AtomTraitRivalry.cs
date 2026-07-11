namespace FCP.Core.Thoughts;

// Base for the Zealot/Congregant rift: dislike triggers when p has SelfTrait and the other pawn has RivalTrait.
public abstract class ThoughtWorker_AtomTraitRivalry : ThoughtWorker
{
    protected abstract TraitDef SelfTrait { get; }
    protected abstract TraitDef RivalTrait { get; }

    protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn otherPin)
    {
        if (SelfTrait == null || RivalTrait == null)
            return ThoughtState.Inactive;

        if (p.story?.traits == null || otherPin.story?.traits == null)
            return ThoughtState.Inactive;

        if (!p.story.traits.HasTrait(SelfTrait) || !otherPin.story.traits.HasTrait(RivalTrait))
            return ThoughtState.Inactive;

        return ThoughtState.ActiveAtStage(0);
    }
}

public class ThoughtWorker_ZealotOfAtom_Social : ThoughtWorker_AtomTraitRivalry
{
    protected override TraitDef SelfTrait => FCPDefOf.FCP_Trait_ZealotOfAtom;
    protected override TraitDef RivalTrait => FCPDefOf.FCP_Trait_CongregantOfAtom;
}

public class ThoughtWorker_CongregantOfAtom_Social : ThoughtWorker_AtomTraitRivalry
{
    protected override TraitDef SelfTrait => FCPDefOf.FCP_Trait_CongregantOfAtom;
    protected override TraitDef RivalTrait => FCPDefOf.FCP_Trait_ZealotOfAtom;
}
