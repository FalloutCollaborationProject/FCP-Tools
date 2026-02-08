using System.Reflection;
using HarmonyLib;
// ReSharper disable InconsistentNaming

namespace FCP.Core.Access;

[StaticConstructorOnStartup]
public static class AccessExtensions_WorkGiver_DoBill
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

    private static readonly TryFindBestIngredientsHelper_Delegate TryFindBestIngredientsHelper =
        AccessTools.MethodDelegate<TryFindBestIngredientsHelper_Delegate>(
            AccessTools.Method(typeof(WorkGiver_DoBill), "TryFindBestIngredientsHelper"));

    private static readonly TryFindBestIngredientsInSet_NoMixHelper_Delegate TryFindBestIngredientsInSet_NoMixHelper =
        AccessTools.MethodDelegate<TryFindBestIngredientsInSet_NoMixHelper_Delegate>(
            AccessTools.Method(typeof(WorkGiver_DoBill), "TryFindBestIngredientsInSet_NoMixHelper"));
    
    static AccessExtensions_WorkGiver_DoBill() {}

    public static bool P_TryFindBestIngredientsHelper(
        Predicate<Thing> thingValidator,
        Predicate<List<Thing>> foundAllIngredientsAndChoose,
        List<IngredientCount> ingredients,
        Pawn pawn,
        Thing billGiver,
        List<ThingCount> chosen,
        float searchRadius)
        => TryFindBestIngredientsHelper(
            thingValidator, foundAllIngredientsAndChoose, ingredients, pawn, billGiver, chosen, searchRadius);

    public static bool P_TryFindBestIngredientsInSet_NoMixHelper(
        List<Thing> availableThings,
        List<IngredientCount> ingredients,
        List<ThingCount> chosen,
        IntVec3 rootCell,
        bool alreadySorted,
        List<IngredientCount> missingIngredients,
        Bill bill = null)
        => TryFindBestIngredientsInSet_NoMixHelper(
            availableThings, ingredients, chosen, rootCell, alreadySorted, missingIngredients, bill);
}