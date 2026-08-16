using UnityEngine;

namespace FCP.Core.Buildings;

public class CompProperties_FoodPreserver : CompProperties
{
    public float rotRateFactor = 0.4f;

    public CompProperties_FoodPreserver()
    {
        compClass = typeof(CompFoodPreserver);
    }
}

public class CompFoodPreserver : ThingComp
{
    private CompPowerTrader powerComp;

    public CompProperties_FoodPreserver Props => (CompProperties_FoodPreserver)props;

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        powerComp = parent.GetComp<CompPowerTrader>();
    }

    public override void CompTickRare()
    {
        base.CompTickRare();

        if (powerComp != null && !powerComp.PowerOn)
            return;

        if (parent is not Building_Storage storage)
            return;

        float naturalRotThisInterval = GenTicks.TickRareInterval * GenTemperature.RotRateAtTemperature(storage.AmbientTemperature);
        float reduction = naturalRotThisInterval * (1f - Props.rotRateFactor);
        if (reduction <= 0f)
            return;

        SlotGroup slotGroup = storage.GetSlotGroup();
        if (slotGroup == null)
            return;

        foreach (Thing thing in slotGroup.HeldThings)
        {
            CompRottable rot = thing.TryGetComp<CompRottable>();
            if (rot != null)
                rot.RotProgress = Mathf.Max(0f, rot.RotProgress - reduction);
        }
    }
}
