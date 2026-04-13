using HarmonyLib;
// ReSharper disable InconsistentNaming

namespace FCP.Core.Access;

[StaticConstructorOnStartup]
internal static class RefuelWorkGiverUtility_Access
{
    private delegate Thing FindBestFuel_Delegate(Pawn pawn, Thing refuelable);

    private delegate List<Thing> FindAllFuel_Delegate(Pawn pawn, Thing refuelable);

    private static readonly FindBestFuel_Delegate findBestFuelDelegate =
        AccessTools.MethodDelegate<FindBestFuel_Delegate>(
            AccessTools.Method(typeof(RefuelWorkGiverUtility), "FindBestFuel"));

    private static readonly FindAllFuel_Delegate findAllFuelDelegate =
        AccessTools.MethodDelegate<FindAllFuel_Delegate>(
            AccessTools.Method(typeof(RefuelWorkGiverUtility), "FindAllFuel"));

    static RefuelWorkGiverUtility_Access() {}

    [CanBeNull]
    internal static Thing FindBestFuel(Pawn pawn, Thing refuelable) => findBestFuelDelegate(pawn, refuelable);

    [CanBeNull]
    internal static List<Thing> FindAllFuel(Pawn pawn, Thing refuelable) => findAllFuelDelegate(pawn, refuelable);
}

