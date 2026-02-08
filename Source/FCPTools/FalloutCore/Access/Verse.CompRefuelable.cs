using System.Reflection;
using HarmonyLib;

// ReSharper disable InconsistentNaming

namespace FCP.Core.Access;

[StaticConstructorOnStartup]
internal static class AccessExtensions_CompRefuelable
{
    private static readonly MethodInfo m_ConsumptionRatePerTick_Get =
        AccessTools.PropertyGetter(typeof(CompRefuelable), "ConsumptionRatePerTick");

    private static readonly Func<CompRefuelable, float> ConsumptionRatePerTick_Get =
        AccessTools.MethodDelegate<Func<CompRefuelable, float>>(m_ConsumptionRatePerTick_Get);

    static AccessExtensions_CompRefuelable()
    {
    }

    internal static float P_ConsumptionRatePerTick(this CompRefuelable comp) 
        => ConsumptionRatePerTick_Get(comp);
}