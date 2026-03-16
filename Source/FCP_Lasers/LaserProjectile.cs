using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace FCP.Core.Laser
{
    [HotSwappable]
    public class LaserProjectile : Projectile
    {
        public Vector3 originOld;
        public Vector3 launcherPosOld;
        public float launcherAngleOld;
        public int launchTick;
        public float originAngle;
        public Vector3 originDest;
        private LaserProperties laserProperties;
        public LaserProperties LaserProperties => laserProperties ??= def.GetModExtension<LaserProperties>();
        private float angleOffset;
        private Effecter endEffecter;
        private Sustainer activeSustainer;
        private int sustainerStartTick;

        public override void Launch(Thing launcher, Vector3 origin, LocalTargetInfo usedTarget, 
            LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, bool preventFriendlyFire = false, 
            Thing equipment = null, ThingDef targetCoverDef = null)
        {
            base.Launch(launcher, origin, usedTarget, intendedTarget, hitFlags, preventFriendlyFire, equipment, targetCoverDef);
            launchTick = Find.TickManager.TicksGame;
            originAngle = ExactRotation.eulerAngles.y;
            angleOffset = LaserProperties.sweepRatePerTick;
            originDest = destination;
            launcherPosOld = launcher.DrawPos;
            launcherAngleOld = Utilities.GetBodyAngle(launcher);
            originOld = origin;
        }
        
        private float GetAngleFromTarget(Vector3 target)
        {
            var targetAngle = (origin.Yto0() - target.Yto0()).AngleFlat() - 180f;
            return AngleAdjusted(targetAngle);
        }

        public float AngleAdjusted(float angle)
        {
            if (angle > 360f)
            {
                angle -= 360f;
            }
            if (angle < 0f)
            {
                angle += 360f;
            }
            return angle;
        }

        private Vector2 textureScroll;
        
        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            float arcHeight = ArcHeightFactor * GenMath.InverseParabola(DistanceCoveredFraction);
            Vector3 beamStart = origin;
            Vector3 beamEnd = drawLoc;
            float beamSize;
            
            if (LaserProperties.beamLength > 0)
            {
                float traveled = Vector3.Distance(origin.Yto0(), drawLoc.Yto0());
                if (traveled > LaserProperties.beamLength)
                {
                    Vector3 dir = (drawLoc - origin).normalized;
                    beamEnd = origin + dir * LaserProperties.beamLength;
                    beamSize = LaserProperties.beamLength;
                }
                else
                {
                    beamSize = traveled;
                }
            }
            else
            {
                beamSize = Vector3.Distance(origin.Yto0(), drawLoc.Yto0());
            }
            
            Vector3 drawPos = Vector3.Lerp(beamStart, beamEnd, 0.5f);
            drawPos.y += 1f;
            Vector3 pos = drawPos + new Vector3(0f, 0f, 1f) * arcHeight;
            
            Comps_PostDraw();
            Material mat = DrawMat;
            if (textureScroll != Vector2.zero)
            {
                mat.SetTextureOffset("_MainTex", textureScroll);
            }
            
            float width = LaserProperties.beamWidth * LaserProperties.beamWidthDrawScale;
            Graphics.DrawMesh(MeshPool.GridPlane(new Vector2(width, beamSize)), pos, ExactRotation, mat, 0);
        }

        public override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            if (LaserProperties.groundFleckDef != null)
            {
                FleckMaker.Static(destination, Map, LaserProperties.groundFleckDef);
            }
            
            if (hitThing != null)
            {
                ImpactOverride(hitThing, blockedByShield);
            }
            
            shouldBeDestroyed = true;
            Destroy(DestroyMode.Vanish);
        }

        protected void ImpactOverride(Thing hitThing, bool blockedByShield = false)
        {
            var battleLog = new BattleLogEntry_RangedImpact(launcher, hitThing, intendedTarget.Thing, equipmentDef, def, targetCoverDef);
            Find.BattleLog.Add(battleLog);
            
            if (hitThing == null) return;
            
            bool instigatorGuilty = !(launcher is Pawn pawn) || !pawn.Drafted;
            var dinfo = new DamageInfo(def.projectile.damageDef, DamageAmount, ArmorPenetration, 
                ExactRotation.eulerAngles.y, launcher, null, equipmentDef, 
                DamageInfo.SourceCategory.ThingOrUnknown, intendedTarget.Thing, instigatorGuilty);
            dinfo.SetWeaponQuality(equipmentQuality);
            hitThing.TakeDamage(dinfo).AssociateWithLog(battleLog);
            
            if (def.projectile.extraDamages != null)
            {
                foreach (var extra in def.projectile.extraDamages)
                {
                    if (Rand.Chance(extra.chance))
                    {
                        var extraInfo = new DamageInfo(extra.def, extra.amount, extra.AdjustedArmorPenetration(), 
                            ExactRotation.eulerAngles.y, launcher, null, equipmentDef, 
                            DamageInfo.SourceCategory.ThingOrUnknown, intendedTarget.Thing, instigatorGuilty);
                        hitThing.TakeDamage(extraInfo).AssociateWithLog(battleLog);
                    }
                }
            }
            
            if (Rand.Chance(DamageDef.igniteCellChance))
            {
                FireUtility.TryStartFireIn(Position, Map, Rand.Range(0.55f, 0.85f), launcher);
            }
        }

        private bool shouldBeDestroyed;

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (activeSustainer != null)
            {
                activeSustainer.End();
                activeSustainer = null;
            }
            if (shouldBeDestroyed)
            {
                base.Destroy(mode);
            }
        }

        public override void Tick()
        {
            base.Tick();
            LockOnCaster();
            textureScroll += LaserProperties.textureScrollOffsetPerTick;
            
            if (LaserProperties.damageTickRate > 0 && this.IsHashIntervalTick(LaserProperties.damageTickRate))
            {
                DamageThings();
            }
            
            if (LaserProperties.sweepRatePerTick > 0)
            {
                DoSweep();
            }
            else if (LaserProperties.lockOnTarget)
            {
                destination = intendedTarget.CenterVector3;
            }
            
            IntVec3 cell = ExactPosition.ToIntVec3();
            Vector3 offset = ExactPosition - cell.ToVector3Shifted();
            
            if (endEffecter == null && LaserProperties.endEffecterDef != null)
            {
                endEffecter = LaserProperties.endEffecterDef.Spawn(cell, Map, offset);
            }
            
            if (endEffecter != null)
            {
                endEffecter.offset = offset;
                endEffecter.EffectTick(new TargetInfo(cell, Map), TargetInfo.Invalid);
                endEffecter.ticksLeft--;
            }

            if (LaserProperties.sustainerSoundDef != null)
            {
                if (activeSustainer == null)
                {
                    if (LaserProperties.sustainerTickPeriod <= 0 || sustainerStartTick == 0)
                    {
                        activeSustainer = SoundStarter.TrySpawnSustainer(LaserProperties.sustainerSoundDef, 
                            SoundInfo.InMap(this, MaintenanceType.PerTick));
                        sustainerStartTick = Find.TickManager.TicksGame;
                    }
                }
                else if (LaserProperties.sustainerTickPeriod > 0 && sustainerStartTick > 0 && 
                         Find.TickManager.TicksGame - sustainerStartTick >= LaserProperties.sustainerTickPeriod)
                {
                    activeSustainer.End();
                    activeSustainer = null;
                    sustainerStartTick = -1;
                    if (LaserProperties.trailSoundDef != null)
                    {
                        SoundStarter.PlayOneShot(LaserProperties.trailSoundDef, SoundInfo.InMap(this, MaintenanceType.None));
                    }
                }

                activeSustainer?.Maintain();
            }

            if (Find.TickManager.TicksGame > launchTick + LaserProperties.lifetimeTicks) 
            {
                Explode();
                shouldBeDestroyed = true;
                Destroy();
            }
        }

        private void LockOnCaster()
        {
            float bodyAngle = Utilities.GetBodyAngle(launcher);
            Vector3 drawPos = launcher.DrawPos.Yto0();
            float angleDiff = bodyAngle - launcherAngleOld;
            Vector3 originOffset = (originOld.Yto0() - launcherPosOld.Yto0()).RotatedBy(angleDiff);
            origin = drawPos + originOffset;
        }

        private void DoSweep()
        {
            float origAngle = LaserProperties.lockOnTarget ? GetAngleFromTarget(intendedTarget.CenterVector3) : originAngle;
            float angle = ExactRotation.eulerAngles.y - origAngle;
            
            if (angle > LaserProperties.maxSweepAngle)
            {
                angleOffset = -LaserProperties.sweepRatePerTick;
            }
            else if (angle < 0 && Mathf.Abs(angle) > LaserProperties.maxSweepAngle)
            {
                angleOffset = LaserProperties.sweepRatePerTick;
            }
            
            destination = RotatePointAroundPivot(destination, origin, new Vector3(0, angleOffset, 0));
            
            if (LaserProperties.lockOnTarget)
            {
                float distCurrent = Vector3.Distance(origin.Yto0(), destination.Yto0());
                float distTarget = Vector3.Distance(origin.Yto0(), intendedTarget.CenterVector3.Yto0());
                float diff = distTarget - distCurrent;
                destination += (Vector3.forward * diff).RotatedBy(ExactRotation.eulerAngles.y);
            }
        }

        private List<IntVec3> GetSweepCells()
        {
            var cells = new HashSet<IntVec3>();
            float angleOffset = -LaserProperties.maxSweepAngle;
            Vector3 targetPos = LaserProperties.lockOnTarget ? intendedTarget.CenterVector3 : originDest;
            Vector3 dest = RotatePointAroundPivot(targetPos, origin, new Vector3(0, angleOffset, 0));
            
            cells.Add(dest.ToIntVec3());
            
            while (angleOffset < LaserProperties.maxSweepAngle)
            {
                angleOffset += LaserProperties.sweepRatePerTick;
                dest = RotatePointAroundPivot(dest, origin, new Vector3(0, LaserProperties.sweepRatePerTick, 0));
                cells.Add(dest.ToIntVec3());
            }
            
            if (LaserProperties.explosionRadius > 0)
            {
                var expandedCells = new List<IntVec3>(cells);
                foreach (var cell in expandedCells)
                {
                    cells.AddRange(GenRadial.RadialCellsAround(cell, LaserProperties.explosionRadius, true));
                }
            }
            
            return cells.ToList();
        }

        private Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
        {
            Vector3 dir = point - pivot;
            dir = Quaternion.Euler(angles) * dir;
            return dir + pivot;
        }

        private void DamageThings()
        {
            var impactCells = GetImpactCells(LaserProperties.damageThingsAcrossBeamLine);
            var targets = new HashSet<Thing>();
            
            foreach (var cell in impactCells)
            {
                if (!cell.InBounds(Map)) continue;
                
                targets.AddRange(cell.GetThingList(Map));
                
                if (LaserProperties.debugCells)
                {
                    Map.debugDrawer.FlashCell(cell);
                }
            }
            
            foreach (var thing in targets)
            {
                if (IsDamagable(thing))
                {
                    ImpactOverride(thing);
                }
            }
        }

        private HashSet<IntVec3> GetImpactCells(bool includePath)
        {
            var cells = new HashSet<IntVec3>();
            cells.AddRange(GenRadial.RadialCellsAround(ExactPosition.ToIntVec3(), LaserProperties.beamWidth, true));
            
            if (includePath)
            {
                float distance = Vector3.Distance(origin.Yto0(), ExactPosition.Yto0());
                for (float d = 0; d < distance; d += 0.5f)
                {
                    Vector3 pos = Vector3.MoveTowards(origin, ExactPosition, d);
                    IntVec3 cell = pos.ToIntVec3();
                    if (cells.Add(cell))
                    {
                        cells.AddRange(GenRadial.RadialCellsAround(cell, LaserProperties.beamWidth, true));
                    }
                }
            }
            
            return cells;
        }

        private bool IsDamagable(Thing thing)
        {
            return (thing is Pawn || thing.def.useHitPoints) && thing != launcher 
                && thing is not Projectile && thing is not Filth && thing is not Mote;
        }

        private void Explode()
        {
            if (LaserProperties.explosionOnEnd == null) return;
            
            var cells = LaserProperties.sweepRatePerTick > 0 
                ? GetSweepCells() 
                : GetImpactCells(LaserProperties.damageThingsAcrossBeamLine).ToList();
                
            GenExplosion.DoExplosion(
                center: origin.ToIntVec3(), 
                map: Map, 
                radius: 0f, 
                damType: LaserProperties.explosionOnEnd, 
                instigator: launcher, 
                overrideCells: cells, 
                propagationSpeed: LaserProperties.explosionSpeed);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref launchTick, "launchTick");
            Scribe_Values.Look(ref originAngle, "originAngle");
            Scribe_Values.Look(ref angleOffset, "angleOffset");
            Scribe_Values.Look(ref originDest, "originDest");
            Scribe_Values.Look(ref launcherPosOld, "launcherPosOld");
            Scribe_Values.Look(ref launcherAngleOld, "launcherAngleOld");
            Scribe_Values.Look(ref sustainerStartTick, "sustainerStartTick");
        }
    }
}
