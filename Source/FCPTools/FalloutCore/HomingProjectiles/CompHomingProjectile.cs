using HarmonyLib;
using UnityEngine;

namespace FCP.Core;

public class CompProperties_HomingProjectile : CompProperties
{
    public float initialSpreadAngle;
    public int tickRate;
    public float turnRate;
    public int lifetimeTicks;
    public int delayTurnInTicks;
    public FleckDef tailFleck;
    public ThingDef tailMote;
    public int? effectLifetime;
    public float effectSize = 1f;

    public CompProperties_HomingProjectile()
    {
        compClass = typeof(CompHomingProjectile);
    }
}

[HotSwappable]
public class CompHomingProjectile : ThingComp
{
    public Vector3 originLaunchCell;
    public int launchTick;
    public bool isOffset;
    
    public Projectile Projectile => parent as Projectile;
    public CompProperties_HomingProjectile Props => props as CompProperties_HomingProjectile;
    public Vector3 DispersionOffset => new Vector3(
        Rand.Range(-Props.initialSpreadAngle, Props.initialSpreadAngle), 
        0f, 
        Rand.Range(-Props.initialSpreadAngle, Props.initialSpreadAngle));

    public override void PostDestroy(DestroyMode mode, Map previousMap)
    {
        base.PostDestroy(mode, previousMap);
        Log.Message("Destroyed: " + this);
    }

    public bool CanChangeTrajectory(out bool delayTurning)
    {
        delayTurning = false;
        var projectile = Projectile;
        if (projectile.intendedTarget.Thing is Pawn pawn && pawn.Dead)
        {
            return false;
        }
        if (Props.delayTurnInTicks > 0 && Find.TickManager.TicksGame - launchTick < Props.delayTurnInTicks)
        {
            delayTurning = true;
            return false;
        }
        return Find.TickManager.TicksGame % Props.tickRate == 0;
    }

    public override void CompTick()
    {
        base.CompTick();
        if (parent.Map != null)
        {
            var travProj = Traverse.Create(Projectile);
            float arcHeight = travProj.Property("ArcHeightFactor").GetValue<float>();
            float distCovered = travProj.Property("DistanceCoveredFraction").GetValue<float>();
            float num = arcHeight * GenMath.InverseParabola(distCovered);
            Vector3 drawPos = Projectile.DrawPos;
            Vector3 position = drawPos + new Vector3(0f, 0f, 1f) * num;
            Vector3 origin = travProj.Field("origin").GetValue<Vector3>();
            ThrowEffect(position, Projectile.Map, Vector3.Angle(origin, position), Props.effectSize);
        }
    }

    public void ThrowEffect(Vector3 loc, Map map, float angle, float size)
    {
        if (loc.InBounds(map))
        {
            float distCovered = Traverse.Create(Projectile).Property("DistanceCoveredFraction").GetValue<float>();
            var solidTimeOverride = Props.effectLifetime.HasValue 
                ? Props.effectLifetime.Value 
                : 0.20f * (1f - (distCovered + 0.1f));
            
            if (Props.tailFleck != null)
            {
                FleckCreationData dataStatic = FleckMaker.GetDataStatic(loc, map, Props.tailFleck, size);
                dataStatic.velocityAngle = angle;
                dataStatic.solidTimeOverride = solidTimeOverride;
                dataStatic.velocitySpeed = 0.01f;
                map.flecks.CreateFleck(dataStatic);
            }
            else if (Props.tailMote != null)
            {
                Mote mote = (Mote)ThingMaker.MakeThing(Props.tailMote);
                mote.exactPosition = loc;
                mote.Scale = size;
                mote.solidTimeOverride = solidTimeOverride;
                if (mote is MoteThrown moteThrown)
                {
                    moteThrown.Speed = 0.01f;
                    moteThrown.MoveAngle = angle;
                }
                GenSpawn.Spawn(mote, loc.ToIntVec3(), map);
            }
        }
    }

    public bool RotateTowards(Vector3 target, out Vector3 destinationRotated)
    {
        destinationRotated = Traverse.Create(Projectile).Field("destination").GetValue<Vector3>();
        float targetAngle = GetAngleFromTarget(target);
        var curAngle = AngleAdjusted(Projectile.ExactRotation.eulerAngles.y + 90);
        float diff = targetAngle - curAngle;
        
        if (new FloatRange(targetAngle - Props.turnRate, targetAngle + Props.turnRate).Includes(curAngle))
        {
            Log.Message($"Projectile.ExactRotation: {Projectile.ExactRotation.eulerAngles.y} - Not Rotating: Diff: {diff} - targetAngle: {targetAngle} - {curAngle} - destination: {destinationRotated} - ExactPosition: {Projectile.ExactPosition}");
            return false;
        }

        var newTarget = Projectile.ExactPosition + (Quaternion.AngleAxis(curAngle, Vector3.up) * Vector3.forward);

        if (diff > 0 ? diff > 180f : diff >= -180f)
        {
            destinationRotated = newTarget.RotatedBy(-Props.turnRate);
            Log.Message($"Projectile.ExactRotation: {Projectile.ExactRotation.eulerAngles.y} - Rotating counterclock: Diff: {diff} - targetAngle: {targetAngle} - {curAngle} - destinationRotated: {destinationRotated} - ExactPosition: {Projectile.ExactPosition}");
        }
        else
        {
            destinationRotated = newTarget.RotatedBy(Props.turnRate);
            Log.Message($"Projectile.ExactRotation: {Projectile.ExactRotation.eulerAngles.y} - Rotating clock: Diff: {diff} - targetAngle: {targetAngle} - {curAngle} - destinationRotated: {destinationRotated} - ExactPosition: {Projectile.ExactPosition}");
        }
        Projectile.Map.debugDrawer.FlashCell(destinationRotated.ToIntVec3());
        return true;
    }

    private float GetAngleFromTarget(Vector3 target)
    {
        var targetAngle = (Projectile.ExactPosition.Yto0() - target.Yto0()).AngleFlat();
        return AngleAdjusted(targetAngle);
    }

    public float AngleAdjusted(float angle)
    {
        return ClampAndWrap(angle, 0, 360);
    }

    public float ClampAndWrap(float val, float min, float max)
    {
        while (val < min || val > max)
        {
            if (val < min)
            {
                val += max;
            }
            if (val > max)
            {
                val -= max;
            }
        }
        return val;
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref originLaunchCell, "originLaunchCell");
        Scribe_Values.Look(ref launchTick, "launchTick");
    }
}
