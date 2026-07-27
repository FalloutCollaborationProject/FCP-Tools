using RimWorld;
using Verse;

namespace FCP.Core.Traits;

public enum TraitStatCondition
{
    DayTime,
    NightTime,
    Stationary,
    LowHealth,
}

public class StatPart_TraitConditional : StatPart
{
    public TraitDef trait;
    public float offset;
    public TraitStatCondition condition;
    public float dayStartHour = 6f;
    public float dayEndHour = 18f;
    public float healthThreshold = 0.3f;
    public bool requireUnroofed;

    public override void TransformValue(StatRequest req, ref float val)
    {
        if (Applies(req))
            val += offset;
    }

    public override string ExplanationPart(StatRequest req)
    {
        if (!Applies(req))
            return null;
        return trait.degreeDatas[0].label.CapitalizeFirst() + ": " + offset.ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Offset);
    }

    private bool Applies(StatRequest req)
    {
        if (req.Thing is not Pawn pawn || pawn.Dead || pawn.story?.traits == null)
            return false;
        if (!pawn.story.traits.HasTrait(trait))
            return false;
        if (requireUnroofed && !TraitConditionUtility.IsUnroofed(pawn))
            return false;

        return condition switch
        {
            TraitStatCondition.DayTime => TraitConditionUtility.IsDayTime(pawn, dayStartHour, dayEndHour),
            TraitStatCondition.NightTime => pawn.Map != null && !TraitConditionUtility.IsDayTime(pawn, dayStartHour, dayEndHour),
            TraitStatCondition.Stationary => TraitConditionUtility.IsStationary(pawn),
            TraitStatCondition.LowHealth => TraitConditionUtility.IsLowHealth(pawn, healthThreshold),
            _ => true,
        };
    }
}
