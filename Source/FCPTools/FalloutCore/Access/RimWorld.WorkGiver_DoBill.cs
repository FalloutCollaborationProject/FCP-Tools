using HarmonyLib;

// ReSharper disable InconsistentNaming

namespace FCP.Core.Access;

[StaticConstructorOnStartup]
internal static class WorkGiver_DoBill_Access
{
    private delegate bool TryFindBestIngredientsHelper_Delegate(
        Predicate<Thing> thingValidator,
        Predicate<List<Thing>> foundAllIngredientsAndChoose,
        List<IngredientCount> ingredients,
        Pawn pawn,
        Thing billGiver,
        List<ThingCount> chosen,
        float searchRadius);

    private delegate bool TryFindBestIngredientsInSet_NoMixHelper_Delegate(
        List<Thing> availableThings,
        List<IngredientCount> ingredients,
        List<ThingCount> chosen,
        IntVec3 rootCell,
        bool alreadySorted,
        List<IngredientCount> missingIngredients,
        Bill bill = null);

    private static readonly TryFindBestIngredientsHelper_Delegate tryFindBestIngredientsHelperDelegate =
        AccessTools.MethodDelegate<TryFindBestIngredientsHelper_Delegate>(
            AccessTools.Method(typeof(WorkGiver_DoBill), "TryFindBestIngredientsHelper"));

    private static readonly TryFindBestIngredientsInSet_NoMixHelper_Delegate
        tryFindBestIngredientsInSet_NoMixHelperDelegate =
            AccessTools.MethodDelegate<TryFindBestIngredientsInSet_NoMixHelper_Delegate>(
                AccessTools.Method(typeof(WorkGiver_DoBill), "TryFindBestIngredientsInSet_NoMixHelper"));

    static WorkGiver_DoBill_Access() { }

    internal static bool TryFindBestIngredientsHelper(Predicate<Thing> thingValidator,
        Predicate<List<Thing>> foundAllIngredientsAndChoose, List<IngredientCount> ingredients, Pawn pawn,
        Thing billGiver, List<ThingCount> chosen, float searchRadius) 
        => tryFindBestIngredientsHelperDelegate(thingValidator, foundAllIngredientsAndChoose, ingredients, pawn, billGiver, chosen, searchRadius);

    internal static bool TryFindBestIngredientsInSet_NoMixHelper(List<Thing> availableThings,
        List<IngredientCount> ingredients, List<ThingCount> chosen, IntVec3 rootCell, bool alreadySorted,
        List<IngredientCount> missingIngredients, Bill bill = null) 
        => tryFindBestIngredientsInSet_NoMixHelperDelegate(availableThings, ingredients, chosen, rootCell, alreadySorted, missingIngredients, bill);
}