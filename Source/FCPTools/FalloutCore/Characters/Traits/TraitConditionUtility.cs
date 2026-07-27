using Verse;

namespace FCP.Core.Traits;

public static class TraitConditionUtility
{
    public static bool IsDayTime(Pawn pawn, float dayStartHour, float dayEndHour)
    {
        if (pawn.Map == null)
            return false;
        float hour = GenLocalDate.HourOfDay(pawn);
        return hour >= dayStartHour && hour < dayEndHour;
    }

    public static bool IsUnroofed(Pawn pawn)
    {
        return pawn.Map != null && !pawn.Position.Roofed(pawn.Map);
    }

    public static bool IsStationary(Pawn pawn)
    {
        return pawn.pather == null || !pawn.pather.MovingNow;
    }

    public static bool IsLowHealth(Pawn pawn, float threshold)
    {
        return pawn.health.summaryHealth.SummaryHealthPercent < threshold;
    }
}
