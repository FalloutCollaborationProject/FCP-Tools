using UnityEngine;
using Verse.AI;

namespace FCP.Core;

public class FloatMenuProvide_PickUp : FloatMenuOptionProvider
{
    protected override bool Drafted => true;
    protected override bool Undrafted => true;
    protected override bool Multiselect => false;

    public override IEnumerable<FloatMenuOption> GetOptionsFor(Thing clickedThing, FloatMenuContext context)
    {
        Thing thing = clickedThing;
        Pawn pawn = context.FirstSelectedPawn;
        if (!thing.def.EverHaulable)
            yield break;

        float mass = thing.GetStatValue(StatDefOf.Mass);
        float freeSpace = MassUtility.FreeSpace(pawn);

        if (!pawn.CanReach(thing, PathEndMode.ClosestTouch, Danger.Deadly))
        {
            yield return new FloatMenuOption("FCP_CannotPickUp".Translate(thing.LabelShort, thing) + ": " + "NoPath".Translate().CapitalizeFirst(), null);
            yield break;
        }
        if (thing.IsBurning())
        {
            yield return new FloatMenuOption("FCP_CannotPickUp".Translate(thing.LabelShort, thing) + ": " + "Burning".Translate(), null);
            yield break;
        }
        if (freeSpace <= mass)
        {
            yield return new FloatMenuOption("FCP_CannotPickUp".Translate(thing.Label, thing) + ": " + "FCP_TooHeavy".Translate(), null);
            yield break;
        }

        bool stackable = thing.def.stackLimit > 1;

        if (!stackable)
        {
            yield return FloatMenuUtility.DecoratePrioritizedTask(
                new FloatMenuOption("FCP_PickUp".Translate(thing.LabelShort, thing), () => DoPickUp(pawn, thing, 1), MenuOptionPriority.High),
                pawn, thing);
            yield break;
        }

        yield return FloatMenuUtility.DecoratePrioritizedTask(
            new FloatMenuOption("FCP_PickUpOne".Translate(thing.LabelShort, thing), () => DoPickUp(pawn, thing, 1), MenuOptionPriority.High),
            pawn, thing);

        if (freeSpace >= mass * thing.stackCount)
        {
            yield return FloatMenuUtility.DecoratePrioritizedTask(
                new FloatMenuOption("FCP_PickUp".Translate(thing.LabelShort, thing), () => DoPickUp(pawn, thing, thing.stackCount), MenuOptionPriority.High),
                pawn, thing);
        }
        else
        {
            int max = Mathf.FloorToInt(freeSpace / mass);
            yield return FloatMenuUtility.DecoratePrioritizedTask(
                new FloatMenuOption("FCP_PickUpMax".Translate(thing.LabelShort, thing), () => DoPickUp(pawn, thing, max), MenuOptionPriority.High),
                pawn, thing);
        }
    }

    static void DoPickUp(Pawn pawn, Thing thing, int count)
    {
        if (thing.HasComp<CompForbiddable>())
            thing.SetForbidden(false);
        Job job = JobMaker.MakeJob(JobDefOf.TakeCountToInventory, thing);
        job.count = count;
        pawn.jobs.TryTakeOrderedJob(job);
    }
}
