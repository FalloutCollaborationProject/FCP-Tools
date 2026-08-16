namespace FCP.Core.Ghouls;

public class ThoughtWorker_RadiationBath : ThoughtWorker
{
    protected override ThoughtState CurrentStateInternal(Pawn p)
    {
        if (p.Map == null || !p.health.hediffSet.HasHediff(HediffDefOf_Ghoul.ToxicHealing))
            return ThoughtState.Inactive;

        return p.Map.gameConditionManager.ConditionIsActive(GameConditionDefOf.ToxicFallout)
            ? ThoughtState.ActiveDefault
            : ThoughtState.Inactive;
    }
}
