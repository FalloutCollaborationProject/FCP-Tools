using System.Linq;
using Verse.AI;

namespace FCP.Core.Turrets;

public class JobDriver_InstallTurretWeapon : JobDriver
{
    private Thing Weapon => job.GetTarget(TargetIndex.A).Thing;
    private Building_TurretGun Turret => (Building_TurretGun)job.GetTarget(TargetIndex.B).Thing;
    private CompTurretWeaponMount Mount => Turret.GetComp<CompTurretWeaponMount>();

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        if (Mount == null || Mount.Armed)
        {
            return false;
        }
        if (CountAvailableComponents() < Mount.Props.componentCost)
        {
            return false;
        }
        return pawn.Reserve(Weapon, job, errorOnFailed: errorOnFailed) && pawn.Reserve(Turret, job, errorOnFailed: errorOnFailed);
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDestroyedOrNull(TargetIndex.A);
        this.FailOnDestroyedOrNull(TargetIndex.B);
        this.FailOn(() => Mount == null || Mount.Armed || Mount.ReservedWeapon != Weapon);

        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);
        yield return Toils_Haul.StartCarryThing(TargetIndex.A);
        yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch);

        Toil install = Toils_General.Wait(Mount.Props.installWorkTicks);
        install.WithProgressBarToilDelay(TargetIndex.B);
        install.FailOnDespawnedOrNull(TargetIndex.B);
        yield return install;

        Toil finish = ToilMaker.MakeToil("FinishInstall");
        finish.initAction = () =>
        {
            ThingDef weaponDef = Weapon.def;
            if (!TryConsumeComponents(Mount.Props.componentCost))
            {
                return;
            }
            pawn.carryTracker.CarriedThing?.Destroy();
            Mount.FinishInstall(weaponDef);
        };
        finish.defaultCompleteMode = ToilCompleteMode.Instant;
        yield return finish;
    }

    private int CountAvailableComponents() =>
        pawn.Map.listerThings.ThingsOfDef(ThingDefOf.ComponentIndustrial)
            .Where(t => !t.IsForbidden(Faction.OfPlayer))
            .Sum(t => t.stackCount);

    private bool TryConsumeComponents(int count)
    {
        int remaining = count;
        foreach (Thing t in pawn.Map.listerThings.ThingsOfDef(ThingDefOf.ComponentIndustrial).ToList())
        {
            if (remaining <= 0)
            {
                break;
            }
            if (t.IsForbidden(Faction.OfPlayer))
            {
                continue;
            }
            int take = System.Math.Min(remaining, t.stackCount);
            t.SplitOff(take).Destroy();
            remaining -= take;
        }
        return remaining <= 0;
    }
}
