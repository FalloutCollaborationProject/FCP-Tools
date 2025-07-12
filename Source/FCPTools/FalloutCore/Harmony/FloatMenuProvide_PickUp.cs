using RimWorld.Planet;
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
        var thing = clickedThing;
        var pawn = context.FirstSelectedPawn;
        if (thing.def.EverHaulable)
        {

            float mass = thing.GetStatValue(StatDefOf.Mass);


            //pick up one
            if (pawn.CanReach((LocalTargetInfo)thing, PathEndMode.ClosestTouch, Danger.Deadly) && !thing.IsBurning() && (MassUtility.FreeSpace(pawn) > mass))
            {
                FloatMenuOption floatMenuOption1 = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption((string)"FCP_PickUpOne".Translate((NamedArgument)thing.LabelShort, (NamedArgument)thing), (Action)(() =>
                {
                    DoPickUp(pawn, thing, 1);
                }),
                MenuOptionPriority.High), pawn, (LocalTargetInfo)thing);
                yield return floatMenuOption1;



                //pick up all
                FloatMenuOption floatMenuOption =
                    pawn.CanReach((LocalTargetInfo)thing, PathEndMode.ClosestTouch, Danger.Deadly) ?
                    (!thing.IsBurning() ?
                    ((MassUtility.FreeSpace(pawn) > mass * thing.stackCount) ?
                    FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption((string)"FCP_PickUp".Translate((NamedArgument)thing.LabelShort, (NamedArgument)thing), (Action)(() =>
                    {

                        DoPickUp(pawn, thing, thing.stackCount);
                    }),
                    MenuOptionPriority.High), pawn, (LocalTargetInfo)thing) :
                    FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption((string)("FCP_PickUpMax".Translate((NamedArgument)thing.LabelShort, (NamedArgument)thing)), (Action)(() =>
                    {
                        int max = Mathf.FloorToInt(MassUtility.FreeSpace(pawn) / mass);
                        DoPickUp(pawn, thing, max);
                    }),
                    MenuOptionPriority.High), pawn, (LocalTargetInfo)thing)) :
                    new FloatMenuOption((string)("FCP_CannotPickUp".Translate((NamedArgument)thing.LabelShort, (NamedArgument)thing) + ": " + "Burning".Translate()), (Action)null)) :
                    new FloatMenuOption((string)("FCP_CannotPickUp".Translate((NamedArgument)thing.LabelShort, (NamedArgument)thing) + ": " + "NoPath".Translate().CapitalizeFirst()), (Action)null);
                yield return floatMenuOption;
            }
            else
            {
                yield return new FloatMenuOption((string)("FCP_CannotPickUp".Translate((NamedArgument)thing.Label, (NamedArgument)thing) + ": " + "FCP_TooHeavy".Translate()), (Action)null);
            }
        }
    }


    static void DoPickUp(Pawn pawn, Thing thing, int count)
    {
        if (thing.HasComp<CompForbiddable>())
        {
            thing.SetForbidden(false);
        }
        Job job = JobMaker.MakeJob(JobDefOf.TakeCountToInventory, (LocalTargetInfo)thing);

        job.count = count;
        pawn.jobs.TryTakeOrderedJob(job);
    }
}
