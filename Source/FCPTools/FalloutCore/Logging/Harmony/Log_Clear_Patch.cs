using HarmonyLib;

namespace FCP.Core.Logging;

[HarmonyPatch(typeof(Log), nameof(Log.Clear))]
internal static class Log_Clear_Patch
{
    [HarmonyPostfix]
    internal static void Postfix() => FCPLog.verboseCount = 0;
}
