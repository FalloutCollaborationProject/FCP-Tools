using Verse.AI;

namespace FCP.Core.WeaponCondition;

public abstract class RecipeWorker_RepairApparel : RecipeWorker
{
    protected abstract bool IsArmor { get; }

    public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
    {
        CompApparelBench slot = thing.TryGetComp<CompApparelBench>();
        if (slot?.LoadedApparel == null) return false;
        if (!slot.LoadedApparel.def.defName.StartsWith("FCP_")) return false;
        if (slot.LoadedApparel.HitPoints >= slot.LoadedApparel.MaxHitPoints) return false;
        bool isArmor = slot.LoadedApparel.def.thingCategories?.Any(
            c => c.defName is "ApparelArmor" or "ArmorHeadgear") ?? false;
        return isArmor == IsArmor;
    }

    public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
    {
        CompApparelBench slot = billDoer.CurJob.GetTarget(TargetIndex.A).Thing?.TryGetComp<CompApparelBench>();
        if (slot?.LoadedApparel == null) return;
        Thing apparel = slot.LoadedApparel;
        apparel.HitPoints = apparel.MaxHitPoints;
        billDoer.skills.Learn(SkillDefOf.Crafting, 0.5f);
        Messages.Message("FCP_ApparelBench_Done".Translate(billDoer.LabelShort, apparel.LabelShort),
            billDoer, MessageTypeDefOf.PositiveEvent, historical: false);
        slot.Eject();
    }
}

public class RecipeWorker_RepairClothing : RecipeWorker_RepairApparel
{
    protected override bool IsArmor => false;
}

public class RecipeWorker_RepairArmor : RecipeWorker_RepairApparel
{
    protected override bool IsArmor => true;
}
