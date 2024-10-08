using System.Collections.Generic;
using Verse;

namespace FCP.Core.Birds;

public class CompProperties_FlyingPawn : CompProperties
{
    public readonly float evadeChanceWhenFlying = 1f;
    public AttackDamageFactor attackDamageFactor;

    public bool attackEnemiesMasterAttacking;

    public List<GraphicData> flyingBodyGraphicData;

    public List<GraphicData> flyingFemaleBodyGraphicData;

    public float flyingMoveSpeedMultiplier;
    public bool flyWhenFleeing;

    public bool flyWhenHunting;

    public float flyWhenWanderingChance;

    public SoundDef soundOnFly;

    public CompProperties_FlyingPawn()
    {
        compClass = typeof(CompFlyingPawn);
    }

    public override void ResolveReferences(ThingDef parentDef)
    {
        base.ResolveReferences(parentDef);
        if (flyingBodyGraphicData != null)
        {
            foreach (var graphicData in flyingBodyGraphicData)
            {
                graphicData.graphicClass = typeof(Graphic_Multi);
            }
        }

        if (flyingFemaleBodyGraphicData == null)
        {
            return;
        }

        foreach (var graphicData in flyingFemaleBodyGraphicData)
        {
            graphicData.graphicClass = typeof(Graphic_Multi);
        }
    }
}