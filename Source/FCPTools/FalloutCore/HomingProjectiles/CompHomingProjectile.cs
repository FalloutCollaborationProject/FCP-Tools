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
        FCPLog.Verbose($"Destroyed: {this}");
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
        return Props.tickRate <= 1 || Find.TickManager.TicksGame % Props.tickRate == 0;
    }

    private static readonly Func<Projectile, float> ArcHeightFactor =
        AccessTools.MethodDelegate<Func<Projectile, float>>(AccessTools.PropertyGetter(typeof(Projectile), "ArcHeightFactor"));
    private static readonly Func<Projectile, float> DistanceCoveredFraction =
        AccessTools.MethodDelegate<Func<Projectile, float>>(AccessTools.PropertyGetter(typeof(Projectile), "DistanceCoveredFraction"));
    private static readonly AccessTools.FieldRef<Projectile, Vector3> Origin =
        AccessTools.FieldRefAccess<Projectile, Vector3>("origin");

    public override void CompTick()
    {
        base.CompTick();
        if (parent.Map != null)
        {
            Projectile projectile = Projectile;
            float num = ArcHeightFactor(projectile) * GenMath.InverseParabola(DistanceCoveredFraction(projectile));
            Vector3 drawPos = projectile.DrawPos;
            Vector3 position = drawPos + new Vector3(0f, 0f, 1f) * num;
            ThrowEffect(position, projectile.Map, Vector3.Angle(Origin(projectile), position), Props.effectSize);
        }
    }

    public void ThrowEffect(Vector3 loc, Map map, float angle, float size)
    {
        if (loc.InBounds(map))
        {
            float distCovered = DistanceCoveredFraction(Projectile);
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
