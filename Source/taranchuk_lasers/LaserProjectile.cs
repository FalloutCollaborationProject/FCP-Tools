using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace taranchuk_lasers
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
            var num = this.ArcHeightFactor * GenMath.InverseParabola(DistanceCoveredFraction);
            var distanceSize = Vector3.Distance(origin.Yto0(), drawLoc.Yto0());
            var drawPos = Vector3.Lerp(origin, drawLoc, 0.5f);
            drawPos.y += 1f;
            var position = drawPos + new Vector3(0f, 0f, 1f) * num;
            Comps_PostDraw();
            var mat = DrawMat;
            if (textureScroll != Vector2.zero)
            {
                mat.SetTextureOffset("_MainTex", textureScroll);
            }
            Graphics.DrawMesh(MeshPool.GridPlane(new(LaserProperties.beamWidth * LaserProperties.beamWidthDrawScale,
                distanceSize)), position, ExactRotation, DrawMat, 0);
        }

        public override void Impact(Thing hitThing, bool blockedByShield = false)
        {

        }

        protected void ImpactOverride(Thing hitThing, bool blockedByShield = false)
        {
            Map map = base.Map;
            IntVec3 position = base.Position;
            BattleLogEntry_RangedImpact battleLogEntry_RangedImpact = new BattleLogEntry_RangedImpact(launcher, hitThing, intendedTarget.Thing, equipmentDef, def, targetCoverDef);
            Find.BattleLog.Add(battleLogEntry_RangedImpact);
            if (hitThing != null)
            {
                bool instigatorGuilty = !(launcher is Pawn pawn) || !pawn.Drafted;
                DamageInfo dinfo = new DamageInfo(def.projectile.damageDef, DamageAmount, ArmorPenetration, ExactRotation.eulerAngles.y, launcher, null, equipmentDef, DamageInfo.SourceCategory.ThingOrUnknown, intendedTarget.Thing, instigatorGuilty);
                dinfo.SetWeaponQuality(equipmentQuality);
                hitThing.TakeDamage(dinfo).AssociateWithLog(battleLogEntry_RangedImpact);
                Pawn pawn2 = hitThing as Pawn;
                if (def.projectile.extraDamages != null)
                {
                    foreach (ExtraDamage extraDamage in def.projectile.extraDamages)
                    {
                        if (Rand.Chance(extraDamage.chance))
                        {
                            DamageInfo dinfo2 = new DamageInfo(extraDamage.def, extraDamage.amount, extraDamage.AdjustedArmorPenetration(), ExactRotation.eulerAngles.y, launcher, null, equipmentDef, DamageInfo.SourceCategory.ThingOrUnknown, intendedTarget.Thing, instigatorGuilty);
                            hitThing.TakeDamage(dinfo2).AssociateWithLog(battleLogEntry_RangedImpact);
                        }
                    }
                }
                if (Rand.Chance(base.DamageDef.igniteCellChance))
                {
                    FireUtility.TryStartFireIn(base.Position, map, Rand.Range(0.55f, 0.85f), launcher);
                }
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
            else
            {
                if (LaserProperties.lockOnTarget)
                {
                    destination = intendedTarget.CenterVector3;
                }
            }
            if (LaserProperties.groundFleckDef != null && Rand.Chance(LaserProperties.fleckChancePerTick))
            {
                FleckMaker.Static(ExactPosition, Map, LaserProperties.groundFleckDef);
            }

            var intVec = ExactPosition.ToIntVec3();
            Vector3 vector3 = ExactPosition - intVec.ToVector3Shifted();
            if (endEffecter == null && LaserProperties.endEffecterDef != null)
            {
                endEffecter = LaserProperties.endEffecterDef.Spawn(intVec, Map, vector3);
            }
            if (endEffecter != null)
            {
                endEffecter.offset = vector3;
                endEffecter.EffectTick(new TargetInfo(intVec, Map), TargetInfo.Invalid);
                endEffecter.ticksLeft--;
            }

            if (LaserProperties.sustainerSoundDef != null)
            {
                if (activeSustainer == null)
                {
                    if (LaserProperties.sustainerTickPeriod <= 0 || sustainerStartTick == 0)
                    {
                        activeSustainer = SoundStarter.TrySpawnSustainer(LaserProperties.sustainerSoundDef, SoundInfo.InMap(this, MaintenanceType.PerTick));
                        sustainerStartTick = Find.TickManager.TicksGame;
                    }
                }
                else if (LaserProperties.sustainerTickPeriod > 0 && sustainerStartTick > 0 && Find.TickManager.TicksGame - sustainerStartTick >= LaserProperties.sustainerTickPeriod)
                {
                    activeSustainer.End();
                    activeSustainer = null;
                    sustainerStartTick = -1;
                    if (LaserProperties.trailSoundDef != null)
                    {
                        SoundStarter.PlayOneShot(LaserProperties.trailSoundDef, SoundInfo.InMap(this, MaintenanceType.None));
                    }
                }

                if (activeSustainer != null)
                {
                    activeSustainer.Maintain();
                }
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
            var bodyAngle = Utilities.GetBodyAngle(launcher);
            var drawPos = launcher.DrawPos.Yto0();
            var angleDiff = bodyAngle - launcherAngleOld;
            var originOffset = (originOld.Yto0() - launcherPosOld.Yto0()).RotatedBy(angleDiff);
            origin = drawPos + originOffset;
        }

        private void DoSweep()
        {
            var origAngle = LaserProperties.lockOnTarget ? GetAngleFromTarget(intendedTarget.CenterVector3) : originAngle;
            var angle = ExactRotation.eulerAngles.y - origAngle;
            if (angle > LaserProperties.maxSweepAngle)
            {
                angleOffset = -LaserProperties.sweepRatePerTick;
            }
            else if (0 > angle && Mathf.Abs(angle) > LaserProperties.maxSweepAngle)
            {
                angleOffset = LaserProperties.sweepRatePerTick;
            }
            destination = RotatePointAroundPivot(destination, origin, new Vector3(0, angleOffset, 0));
            if (LaserProperties.lockOnTarget)
            {
                var distance = Vector3.Distance(origin.Yto0(), destination.Yto0());
                var distance2 = Vector3.Distance(origin.Yto0(), intendedTarget.CenterVector3.Yto0());
                var diff = distance2 - distance;
                destination += (Vector3.forward * diff).RotatedBy(ExactRotation.eulerAngles.y);
            }
        }

        private List<IntVec3> GetSweepCells()
        {
            var cells = new HashSet<IntVec3>();
            var angleOffsetTmp = -LaserProperties.maxSweepAngle;
            var origDest = LaserProperties.lockOnTarget ? intendedTarget.CenterVector3 : originDest;
            var destTmp = RotatePointAroundPivot(origDest, origin, new Vector3(0, angleOffsetTmp, 0));
            cells.Add(destTmp.ToIntVec3());
            while (true)
            {
                angleOffsetTmp += LaserProperties.sweepRatePerTick;
                if (angleOffsetTmp > LaserProperties.maxSweepAngle)
                {
                    break;
                }
                destTmp = RotatePointAroundPivot(destTmp, origin, new Vector3(0, LaserProperties.sweepRatePerTick, 0));
                cells.Add(destTmp.ToIntVec3());
            }
            if (LaserProperties.explosionRadius > 0)
            {
                foreach (var cell in cells.ToList())
                {
                    cells.AddRange(GenRadial.RadialCellsAround(cell, LaserProperties.explosionRadius, true));
                }
            }
            return cells.ToList();
        }

        private Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
        {
            var dir = point - pivot; // get point direction relative to pivot
            dir = Quaternion.Euler(angles) * dir; // rotate it
            point = dir + pivot; // calculate rotated point
            return point; // return it
        }

        private void DamageThings()
        {
            HashSet<IntVec3> allCellsImpact = GetImpactCells(LaserProperties.damageThingsAcrossBeamLine);
            var thingsToDamage = new HashSet<Thing>();
            foreach (var cell in allCellsImpact)
            {
                if (cell.InBounds(Map))
                {
                    var things = cell.GetThingList(Map);
                    thingsToDamage.AddRange(things);
                    if (LaserProperties.debugCells)
                    {
                        Map.debugDrawer.FlashCell(cell);
                    }
                }
            }
            foreach (var thing in thingsToDamage)
            {
                if (IsDamagable(thing))
                {
                    ImpactOverride(thing);
                }
            }
        }

        private HashSet<IntVec3> GetImpactCells(bool includeCellsAcrossBeamLine)
        {
            var allCellsImpact = new HashSet<IntVec3>();
            allCellsImpact.AddRange(GenRadial.RadialCellsAround(ExactPosition.ToIntVec3(), LaserProperties.beamWidth, true));
            if (includeCellsAcrossBeamLine)
            {
                var distance = Vector3.Distance(origin.Yto0(), ExactPosition.Yto0());
                float dist = 0;
                while (dist < distance)
                {
                    dist += 0.5f;
                    var newPos = Vector3.MoveTowards(origin, ExactPosition, dist);
                    var cell = newPos.ToIntVec3();
                    if (allCellsImpact.Add(cell))
                    {
                        allCellsImpact.AddRange(GenRadial.RadialCellsAround(cell, LaserProperties.beamWidth, true));
                    }
                }
            }
            return allCellsImpact;
        }

        private bool IsDamagable(Thing thing)
        {
            return (thing is Pawn || thing.def.useHitPoints) && thing != launcher 
                && thing is not Projectile && thing is not Filth && thing is not Mote;
        }

        private void Explode()
        {
            if (LaserProperties.explosionOnEnd != null)
            {
                IntVec3 position = origin.ToIntVec3();
                var cells = LaserProperties.sweepRatePerTick > 0 ? GetSweepCells() 
                    : GetImpactCells(LaserProperties.damageThingsAcrossBeamLine).ToList();
                GenExplosion.DoExplosion(center: position, map: Map, radius: 0f, 
                    damType: LaserProperties.explosionOnEnd, 
                    instigator: launcher, 
                    overrideCells: cells, propagationSpeed: LaserProperties.explosionSpeed);
            }
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
