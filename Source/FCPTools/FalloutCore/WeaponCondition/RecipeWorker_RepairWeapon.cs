using Verse.AI;

namespace FCP.Core.WeaponCondition;

public class RecipeWorker_RepairWeapon : RecipeWorker
{
    public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
    {
        CompWeaponBench slot = thing.TryGetComp<CompWeaponBench>();
        if (slot?.LoadedWeapon == null) return false;
        CompWeaponCondition cond = slot.LoadedWeapon.TryGetComp<CompWeaponCondition>();
        return cond != null && cond.Condition < 100f;
    }

    public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
    {
        CompWeaponBench slot = billDoer.CurJob.GetTarget(TargetIndex.A).Thing?.TryGetComp<CompWeaponBench>();
        if (slot?.LoadedWeapon is not ThingWithComps weapon) return;

        CompWeaponCondition comp = weapon.GetComp<CompWeaponCondition>();
        comp.SetCondition(100f);
        comp.ClearJam();
        weapon.HitPoints = weapon.MaxHitPoints;
        billDoer.skills.Learn(SkillDefOf.Crafting, 0.5f);
        Messages.Message("FCP_RepairBench_Done".Translate(billDoer.LabelShort, weapon.LabelShort),
            billDoer, MessageTypeDefOf.PositiveEvent, historical: false);
    }
}
