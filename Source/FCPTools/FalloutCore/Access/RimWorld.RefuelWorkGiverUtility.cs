using HarmonyLib;
// ReSharper disable InconsistentNaming

namespace FCP.Core.Access;

[StaticConstructorOnStartup]
internal static class AccessExtensions_RefuelWorkGiverUtility
{
    private delegate Thing FindBestFuel_Delegate(Pawn pawn, Thing refuelable);

    private delegate List<Thing> FindAllFuel_Delegate(Pawn pawn, Thing refuelable);

    private static readonly FindBestFuel_Delegate FindBestFuel =
        AccessTools.MethodDelegate<FindBestFuel_Delegate>(
            AccessTools.Method(typeof(RefuelWorkGiverUtility), "FindBestFuel"));

    private static readonly FindAllFuel_Delegate FindAllFuel =
        AccessTools.MethodDelegate<FindAllFuel_Delegate>(
            AccessTools.Method(typeof(RefuelWorkGiverUtility), "FindAllFuel"));

    static AccessExtensions_RefuelWorkGiverUtility() {}

    extension(RefuelWorkGiverUtility)
    {
        internal static Thing P_FindBestFuel(Pawn pawn, Thing refuelable)
            => FindBestFuel(pawn, refuelable);

        internal static List<Thing> P_FindAllFuel(Pawn pawn, Thing refuelable)
            => FindAllFuel(pawn, refuelable);
    }
}