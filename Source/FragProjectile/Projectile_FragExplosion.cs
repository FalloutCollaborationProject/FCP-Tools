using UnityEngine;

namespace FragProjectile;

public class Projectile_FragExplosion : Projectile_Explosive
{
    public ProjectileExtension_Fragmentation extension =>  def.GetModExtension<ProjectileExtension_Fragmentation>();
    protected override void Tick()
    {
        base.Tick();
        if(extension.isExplodePreemptively)
        {
            if(ticksToImpact <= extension.tickBeforeImpact)
            {
                Explode();
            }
        }
    }
    protected override void Impact(Thing hitThing, bool blockedByShield = false)
    {
        if(extension.isSureHit)
        {
            if(intendedTarget.HasThing && intendedTarget.Thing.PositionHeld != PositionHeld)
            {
                Launch(launcher, intendedTarget, intendedTarget, ProjectileHitFlags.IntendedTarget);
                return;
            }                                
        }
        base.Impact(hitThing, blockedByShield);
    }
    protected override void Explode()
    {
        Map map = base.Map;
        Destroy();
        if (def.projectile.explosionEffect != null)
        {
            Effecter effecter = def.projectile.explosionEffect.Spawn();
            effecter.Trigger(new TargetInfo(base.PositionHeld, map), new TargetInfo(base.PositionHeld, map), -1);
            effecter.Cleanup();
        }
        IntVec3 positionHeld = base.PositionHeld;
        float explosionRadius = def.projectile.explosionRadius;
        DamageDef damageDef = def.projectile.damageDef;
        Thing thing = launcher;
        int damageAmount = DamageAmount;
        float armorPenetration = ArmorPenetration;
        SoundDef soundExplode = def.projectile.soundExplode;
        ThingDef thingDef = equipmentDef;
        ThingDef thingDef2 = def;
        Thing intendedThing = intendedTarget.Thing;
        ThingDef postExplosionSpawnThingDef = def.projectile.postExplosionSpawnThingDef;
        ThingDef postExplosionSpawnThingDefWater = def.projectile.postExplosionSpawnThingDefWater;
        float postExplosionSpawnChance = def.projectile.postExplosionSpawnChance;
        int postExplosionSpawnThingCount = def.projectile.postExplosionSpawnThingCount;
        GasType? postExplosionGasType = def.projectile.postExplosionGasType;
        ThingDef preExplosionSpawnThingDef = def.projectile.preExplosionSpawnThingDef;
        float preExplosionSpawnChance = def.projectile.preExplosionSpawnChance;
        int preExplosionSpawnThingCount = def.projectile.preExplosionSpawnThingCount;
        GenExplosion.DoExplosion(positionHeld, map, explosionRadius, damageDef, thing, damageAmount, armorPenetration, soundExplode, thingDef, thingDef2, intendedThing, postExplosionSpawnThingDef, postExplosionSpawnChance, postExplosionSpawnThingCount, postExplosionGasType,null,255, def.projectile.applyDamageToExplosionCellsNeighbors, preExplosionSpawnThingDef, preExplosionSpawnChance, preExplosionSpawnThingCount, def.projectile.explosionChanceToStartFire, def.projectile.explosionDamageFalloff, (float?)origin.AngleToFlat(destination), (List<Thing>)null, (FloatRange?)null, true, def.projectile.damageDef.expolosionPropagationSpeed, 0f, true, postExplosionSpawnThingDefWater, def.projectile.screenShakeFactor);
        /*if(extension.isSureHit && intendedThing != null)
        {
            Projectile projectile = (Projectile)GenSpawn.Spawn(extension.sureHitProjectileDef, positionHeld, map);
            projectile.Launch(thing,intendedThing,intendedThing,ProjectileHitFlags.IntendedTarget);
        }*/
        if(extension.isCone)
        {

            IntVec3 finalPos = new IntVec3(Mathf.Max(launcher.PositionHeld.x, positionHeld.x) - Mathf.Min(launcher.PositionHeld.x, positionHeld.x), 0, Mathf.Max(launcher.PositionHeld.z, positionHeld.z) - Mathf.Min(launcher.PositionHeld.z, positionHeld.z));
                
            //if launcher is to the right, multiply offset by -1
            if (thing.PositionHeld.x > positionHeld.x)
            {
                finalPos.x *= -1;
            }
            //if launcher is above, multiply offset by -1
            if (thing.PositionHeld.z > positionHeld.z)
            {
                finalPos.z *= -1;
            }
            IntVec3 rangeEndPosition = finalPos + positionHeld;

            /*Log.Message("launcher" + launcher.PositionHeld);
            Log.Message("impacted" + positionHeld);
            Log.Message("finalPos" + finalPos);
            Log.Message("finalFinalPos" + rangeEndPosition);*/
            for (int i = 0; i < extension.projCount; i++)
            {
                IntVec3 target = GenRadial.RadialCellsAround(rangeEndPosition, extension.radius.RandomInRange, false).RandomElement();
                Projectile projectile = (Projectile)GenSpawn.Spawn(extension.projectileDef, positionHeld, map);
                Thing possibleThing = null;
                foreach(var item in GenSight.PointsOnLineOfSight(positionHeld,target))
                {
                    if(item.GetFirstPawn(map) != null)
                    {
                        possibleThing = item.GetFirstPawn(map);
                        break;
                    }
                }
                if(possibleThing == null)
                {
                    possibleThing = target.GetFirstPawn(map) ?? target.GetFirstBuilding(map) as Thing;
                }
                if (possibleThing != null)
                {
                    projectile.Launch(Launcher, possibleThing, possibleThing, ProjectileHitFlags.All);
                }
                else
                {
                    projectile.Launch(Launcher, target, target, ProjectileHitFlags.All);
                }
            }
        }
        else
        {
            List<IntVec3> possibleTargetCell = new List<IntVec3>();
            for (int i = 0; i < extension.projCount; i++)
            {
                possibleTargetCell.Add(GenRadial.RadialCellsAround(positionHeld, extension.radius.RandomInRange, true).Where(x => GenSight.LineOfSight(positionHeld, x, map)).RandomElement().ClampInsideMap(map));
            }
            for (int j = 0; j < possibleTargetCell.Count; j++)
            {
                Projectile projectile = (Projectile)GenSpawn.Spawn(extension.projectileDef, base.PositionHeld, map);
                Thing possibleThing = null;
                foreach (var item in GenSight.PointsOnLineOfSight(positionHeld, possibleTargetCell[j]))
                {
                    if (item.GetFirstPawn(map) != null)
                    {
                        possibleThing = item.GetFirstPawn(map);
                        break;
                    }
                }
                if (possibleThing == null)
                {
                    possibleThing = possibleTargetCell[j].GetFirstPawn(map) ?? possibleTargetCell[j].GetFirstBuilding(map) as Thing;
                }
                if (possibleThing != null)
                {
                    projectile.Launch(Launcher, possibleThing, possibleThing, ProjectileHitFlags.IntendedTarget);
                }
                else
                {
                    projectile.Launch(Launcher, possibleTargetCell[j], possibleTargetCell[j], ProjectileHitFlags.NonTargetPawns);
                }
            }
        }            
    }
}