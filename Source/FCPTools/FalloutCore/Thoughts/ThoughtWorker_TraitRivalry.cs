namespace FCP.Core.Thoughts;

public abstract class ThoughtWorker_TraitRivalry : ThoughtWorker
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

public class ThoughtWorker_ZealotOfAtom_Social : ThoughtWorker_TraitRivalry
{
    protected override TraitDef SelfTrait => FCPDefOf.FCP_Trait_ZealotOfAtom;
    protected override TraitDef RivalTrait => FCPDefOf.FCP_Trait_CongregantOfAtom;
}

public class ThoughtWorker_CongregantOfAtom_Social : ThoughtWorker_TraitRivalry
{
    protected override TraitDef SelfTrait => FCPDefOf.FCP_Trait_CongregantOfAtom;
    protected override TraitDef RivalTrait => FCPDefOf.FCP_Trait_ZealotOfAtom;
}

public class ThoughtWorker_InstituteLoyalist_DislikesRailroad : ThoughtWorker_TraitRivalry
{
    protected override TraitDef SelfTrait => FCPDefOf.FCP_Trait_Institute_Loyalist;
    protected override TraitDef RivalTrait => FCPDefOf.FCP_Trait_Railroad_Sympathizer;
}

public class ThoughtWorker_InstituteLoyalist_DislikesBoS : ThoughtWorker_TraitRivalry
{
    protected override TraitDef SelfTrait => FCPDefOf.FCP_Trait_Institute_Loyalist;
    protected override TraitDef RivalTrait => FCPDefOf.FCP_Trait_BoS_Zealot;
}

public class ThoughtWorker_RailroadSympathizer_DislikesInstitute : ThoughtWorker_TraitRivalry
{
    protected override TraitDef SelfTrait => FCPDefOf.FCP_Trait_Railroad_Sympathizer;
    protected override TraitDef RivalTrait => FCPDefOf.FCP_Trait_Institute_Loyalist;
}

public class ThoughtWorker_RailroadSympathizer_DislikesBoS : ThoughtWorker_TraitRivalry
{
    protected override TraitDef SelfTrait => FCPDefOf.FCP_Trait_Railroad_Sympathizer;
    protected override TraitDef RivalTrait => FCPDefOf.FCP_Trait_BoS_Zealot;
}

public class ThoughtWorker_BoSZealot_DislikesInstitute : ThoughtWorker_TraitRivalry
{
    protected override TraitDef SelfTrait => FCPDefOf.FCP_Trait_BoS_Zealot;
    protected override TraitDef RivalTrait => FCPDefOf.FCP_Trait_Institute_Loyalist;
}

public class ThoughtWorker_BoSZealot_DislikesRailroad : ThoughtWorker_TraitRivalry
{
    protected override TraitDef SelfTrait => FCPDefOf.FCP_Trait_BoS_Zealot;
    protected override TraitDef RivalTrait => FCPDefOf.FCP_Trait_Railroad_Sympathizer;
}
