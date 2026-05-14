using UnityEngine;
using Verse.AI;

namespace FCP.Core.WeaponCondition;

public class JobDriver_JuryRigWeapon : JobDriver
{
    private ThingWithComps GroundWeapon => (ThingWithComps)job.targetA.Thing;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(GroundWeapon, job, 1, -1, null, errorOnFailed);
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDestroyedOrNull(TargetIndex.A);
        this.FailOn(() => pawn.equipment.Primary == null || pawn.equipment.Primary.def != GroundWeapon.def);

        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);

        Toil rig = ToilMaker.MakeToil("JuryRig");
        rig.defaultCompleteMode = ToilCompleteMode.Instant;
        rig.initAction = delegate
        {
            ThingWithComps ground = GroundWeapon;
            ThingWithComps equipped = pawn.equipment.Primary;

            CompWeaponCondition equippedComp = equipped.GetComp<CompWeaponCondition>();
            CompWeaponCondition groundComp = ground.GetComp<CompWeaponCondition>();
            float combined = Mathf.Min(100f, equippedComp.Condition + groundComp.Condition - 5f);
            equippedComp.SetCondition(combined);
            equippedComp.ClearJam();
            equipped.HitPoints = Mathf.Max(1, Mathf.RoundToInt(equipped.MaxHitPoints * (combined / 100f)));

            ground.Destroy();

            pawn.skills.Learn(SkillDefOf.Crafting, 0.15f);
            Messages.Message("FCP_JuryRig_MessageDone".Translate(pawn.LabelShort, equipped.LabelShort, equippedComp.Condition.ToString("F0")),
                pawn, MessageTypeDefOf.PositiveEvent, historical: false);
        };
        yield return rig;
    }
}
