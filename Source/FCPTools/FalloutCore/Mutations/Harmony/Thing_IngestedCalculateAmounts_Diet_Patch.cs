using HarmonyLib;

namespace FCP.Mutations;

[HarmonyPatch(typeof(Thing), "IngestedCalculateAmounts")]
public static class Thing_IngestedCalculateAmounts_Diet_Patch
{
    private const float BonusFactor = 1.5f;
    private const float PenaltyFactor = 0.5f;

    private enum DietClass { Unknown, Meat, Plant }

    public static void Postfix(Thing __instance, Pawn ingester, ref float nutritionIngested)
    {
        if (ingester?.genes == null || nutritionIngested <= 0f)
            return;

        bool isCarnivore = ingester.genes.HasActiveGene(MutationsDefOf.FCP_Gene_Carnivore);
        bool isHerbivore = ingester.genes.HasActiveGene(MutationsDefOf.FCP_Gene_Herbivore);
        if (!isCarnivore && !isHerbivore)
            return;

        DietClass diet = ClassifyFood(__instance);
        if (diet == DietClass.Unknown)
            return;

        if (isCarnivore)
            nutritionIngested *= diet == DietClass.Meat ? BonusFactor : PenaltyFactor;
        else
            nutritionIngested *= diet == DietClass.Plant ? BonusFactor : PenaltyFactor;
    }

    private static DietClass ClassifyFood(Thing food)
    {
        CompIngredients ingredientsComp = food.TryGetComp<CompIngredients>();
        if (ingredientsComp != null && ingredientsComp.ingredients.Count > 0)
        {
            int meatCount = 0;
            int plantCount = 0;
            foreach (ThingDef ingredient in ingredientsComp.ingredients)
                Tally(ingredient, ref meatCount, ref plantCount);
            if (meatCount == 0 && plantCount == 0)
                return DietClass.Unknown;
            return meatCount >= plantCount ? DietClass.Meat : DietClass.Plant;
        }

        int m = 0, p = 0;
        Tally(food.def, ref m, ref p);
        if (m > 0)
            return DietClass.Meat;
        if (p > 0)
            return DietClass.Plant;
        return DietClass.Unknown;
    }

    private static void Tally(ThingDef def, ref int meatCount, ref int plantCount)
    {
        FoodTypeFlags foodType = def.ingestible?.foodType ?? FoodTypeFlags.None;
        if ((foodType & FoodTypeFlags.Meat) != 0)
            meatCount++;
        else if ((foodType & (FoodTypeFlags.VegetableOrFruit | FoodTypeFlags.Plant)) != 0)
            plantCount++;
    }
}
