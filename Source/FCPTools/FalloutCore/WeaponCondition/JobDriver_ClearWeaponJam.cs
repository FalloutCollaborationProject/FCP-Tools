using Verse.AI;

namespace FCP.Core.WeaponCondition;

public class JobDriver_ClearWeaponJam : JobDriver
{
    public override bool TryMakePreToilReservations(bool errorOnFailed) => true;

    protected override IEnumerable<Toil> MakeNewToils()
    {
        Toil wait = ToilMaker.MakeToil("ClearJam");
        wait.initAction = delegate { pawn.pather.StopDead(); };
        wait.defaultDuration = 180;
        wait.defaultCompleteMode = ToilCompleteMode.Delay;
        wait.AddFinishAction(delegate
        {
            pawn.equipment?.Primary?.GetComp<CompWeaponCondition>()?.ClearJam();
        });
        yield return wait;
    }
}
