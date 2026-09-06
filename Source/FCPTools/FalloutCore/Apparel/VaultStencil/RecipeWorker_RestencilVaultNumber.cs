using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace FCP.Core;

public class RecipeWorker_RestencilVaultNumber : RecipeWorker
{
    public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
    {
        WeaponCondition.CompApparelBench slot = thing.TryGetComp<WeaponCondition.CompApparelBench>();
        return slot?.LoadedApparel?.TryGetComp<CompVaultNumberStencil>() != null;
    }

    public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
    {
        WeaponCondition.CompApparelBench slot = billDoer.CurJob.GetTarget(TargetIndex.A).Thing?.TryGetComp<WeaponCondition.CompApparelBench>();
        CompVaultNumberStencil stencil = slot?.LoadedApparel?.TryGetComp<CompVaultNumberStencil>();
        if (stencil == null)
            return;

        billDoer.skills.Learn(SkillDefOf.Crafting, 0.5f);
        slot.Eject();
        Find.WindowStack.Add(new Dialog_SetVaultNumber(stencil));
    }
}
