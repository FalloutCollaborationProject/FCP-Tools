using Verse.AI.Group;

namespace FCP.Core;

public class DeathActionWorker_MechDetonatorExplosion : DeathActionWorker
{
    public override RulePackDef DeathRules => RulePackDefOf.Transition_DiedExplosive;

    public override bool DangerousInMelee => true;

    public override void PawnDied(Corpse corpse, Lord prevLord)
    {
        if (corpse.InnerPawn.TryGetComp<CompMechDetonator>(out CompMechDetonator comp))
        {
            GenExplosion.DoExplosion(radius: comp.Props.radius, damAmount: comp.Props.damage, center: corpse.Position, map: corpse.Map, damType: DamageDefOf.Flame, instigator: corpse.InnerPawn);
        }
    }
}