using FCP.Core.VATS;
using Verse.AI;

namespace FCP.Core
{
    public class JobDriver_AttackHybrid : JobDriver
    {
        private bool hasAttacked;
        private bool startedIncapacitated;
        
        public override bool TryMakePreToilReservations(bool errorOnFailed) => true;
        
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref startedIncapacitated, "startedIncapacitated");
        }
        
        private bool TryStartAttack(LocalTargetInfo target)
        {
            if (pawn.stances.FullBodyBusy || pawn.WorkTagIsDisabled(WorkTags.Violent))
                return false;

            bool allowManualCastWeapons = !pawn.IsColonist;
            Verb attackVerb = pawn.TryGetAttackVerb(target.Thing, allowManualCastWeapons);
            
            if (!attackVerb.TryFindShootLineFromTo(TargetThingA.Position, TargetThingB.Position, out ShootLine resultingLine) ||
                !VATS_GameComponent.ActiveAttacks.TryGetValue(pawn, out VATS_GameComponent.VATSAction attack))
                return false;
            
            ThingDef projectileDef = attackVerb.GetProjectile();
            Projectile projectile = (Projectile)GenSpawn.Spawn(projectileDef, resultingLine.Source, TargetThingA.Map);
            
            if (FCPCoreMod.Settings.EnableZoom)
            {
                Thing zoomer = GenSpawn.Spawn(FCPDefOf.FCP_VATS_Zoomer, resultingLine.Source, TargetThingA.Map);
                ((Graphic_Zoomer)zoomer.Graphic).Parent = projectile;
            }
            
            // large refactor here
            // no more messy switch statement with go-to's everywhere
            // replaced with an array lookup to select a miss dir more cleanly
            LocalTargetInfo hitTarget = Rand.Chance(attack.HitChance) 
                ? TargetB 
                : TryGetMissedTarget();
            
            FCPLog.Message($"VATS attack {(hitTarget == TargetB 
                ? "Hit" 
                : "Missed")} to {attack.Target} on {attack.Part} " +
                           $"with hit chance {attack.HitChance}");
            
            projectile.Launch(pawn, pawn.DrawPos, hitTarget, 
                TargetB, ProjectileHitFlags.IntendedTarget,
                false, attack.Equipment);
            return true;
        }
        
        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Misc.ThrowColonistAttackingMote(TargetIndex.A);
            
            Toil initToil = ToilMaker.MakeToil();
            initToil.initAction = () =>
            {
                if (TargetThingA is Pawn targetPawn)
                    startedIncapacitated = targetPawn.Downed;
                
                pawn.pather.StopDead();
            };
            
            // reduced a ton of unnecessary nesting to make this more readable
            // consolidated some checks as well
            initToil.tickAction = () =>
            {
                if (!TargetA.IsValid || (TargetA.HasThing && TargetA.Thing.Destroyed))
                {
                    EndJobWith(JobCondition.Succeeded);
                    return;
                }
                
                if (TargetA.Thing is Pawn targetPawn && !startedIncapacitated && targetPawn.Downed)
                {
                    EndJobWith(JobCondition.Succeeded);
                    return;
                }
                
                if (hasAttacked)
                {
                    EndJobWith(JobCondition.Succeeded);
                    return;
                }
                
                if (TryStartAttack(TargetA))
                {
                    hasAttacked = true;
                    return;
                }
                
                // handle cases where attack fails
                if (pawn.stances.FullBodyBusy) return;
                
                Verb attackVerb = pawn.TryGetAttackVerb(TargetA.Thing, !pawn.IsColonist);
                if (job.endIfCantShootTargetFromCurPos && 
                    (attackVerb == null || !attackVerb.CanHitTargetFrom(pawn.Position, TargetA)))
                {
                    EndJobWith(JobCondition.Incompletable);
                    return;
                }
                
                if (job.endIfCantShootInMelee && attackVerb != null)
                {
                    float minRangeSq = attackVerb.verbProps.EffectiveMinRange(TargetA, pawn) *
                                       attackVerb.verbProps.EffectiveMinRange(TargetA, pawn);
                    
                    if (pawn.Position.DistanceToSquared(TargetA.Cell) < minRangeSq || 
                        !pawn.Position.AdjacentTo8WayOrInside(TargetA.Cell))
                    {
                        EndJobWith(JobCondition.Incompletable);
                    }
                }
            };
            
            initToil.defaultCompleteMode = ToilCompleteMode.Never;
            initToil.activeSkill = () => Toils_Combat.GetActiveSkillForToil(initToil);
            yield return initToil;
        }

        /// <summary>
        /// Determines the missed shot location.
        /// Reduces the number of calls to pawn.TryGetAttackVerb() in the original method.
        /// </summary>
        private IntVec3 TryGetMissedTarget()
        {
            IntVec3[] missOffsets =
            [
                new IntVec3(1, 0, 0), new IntVec3(1, 0, 1),
                new IntVec3(0, 0, 1), new IntVec3(-1, 0, 1),
                new IntVec3(-1, 0, 0), new IntVec3(-1, 0, -1), 
                new IntVec3(0, 0, -1), new IntVec3(1, 0, -1)
            ];
            
            int missDir = Rand.Range(0, 7);
            IntVec3 targetCell = TargetB.Cell + missOffsets[missDir];
            
            return targetCell.InBounds(pawn.Map)
                ? targetCell
                : pawn.Position;
        }
    }
}