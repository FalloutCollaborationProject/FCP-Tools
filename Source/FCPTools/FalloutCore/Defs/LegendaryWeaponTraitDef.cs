using FCP.Core.LegendaryEffectWorkers;

namespace FCP.Core;

public class LegendaryWeaponTraitDef: WeaponTraitDef
{
    public Type legendaryEffectWorkerClass = typeof (LegendaryEffectWorker);

    private LegendaryEffectWorker legendaryEffectWorker;

    public LegendaryEffectWorker LegendaryEffectWorker
    {
        get
        {
            if (!ModLister.CheckRoyaltyOrOdyssey("Weapon traits"))
                return null;
            if (legendaryEffectWorker == null)
            {
                legendaryEffectWorker = (LegendaryEffectWorker) Activator.CreateInstance(legendaryEffectWorkerClass);
                legendaryEffectWorker.def = this;
            }
            return legendaryEffectWorker;
        }
    }
}