namespace FCP.Core;

public class Projectile_Syringe : Bullet
{
    protected override void Impact(Thing hitThing, bool blockedByShield = false)
    {
        Pawn pawn = hitThing as Pawn;
        base.Impact(hitThing, blockedByShield);

        if (pawn == null || pawn.Dead || pawn.health == null)
        {
            return;
        }

        DefExtension_SyringePayload payload = def.GetModExtension<DefExtension_SyringePayload>();
        if (payload == null)
        {
            return;
        }

        if (payload.hediff != null)
        {
            pawn.health.AddHediff(payload.hediff);
        }

        if (payload.mentalState != null && pawn.RaceProps.Humanlike && pawn.mindState != null)
        {
            pawn.mindState.mentalStateHandler.TryStartMentalState(payload.mentalState, forceWake: true, causedByMood: false);
        }
    }
}
