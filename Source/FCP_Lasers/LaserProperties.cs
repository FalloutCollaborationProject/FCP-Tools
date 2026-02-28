using HarmonyLib;
using System;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace FCP.Core.Laser
{
    public class LaserProperties : DefModExtension
    {
        public float beamWidth;
        public float beamWidthDrawScale = 1f;
        public float beamLength = -1f; // Visual length of beam. -1 = full stretch from origin to projectile
        public int damageTickRate;
        public int lifetimeTicks;
        public float sweepRatePerTick;
        public float maxSweepAngle;
        public bool damageThingsAcrossBeamLine;
        public bool debugCells;
        public bool lockOnTarget;
        public DamageDef explosionOnEnd;
        public float explosionSpeed;
        public FleckDef groundFleckDef;
        public float fleckChancePerTick;
        public EffecterDef endEffecterDef;
        public Vector2 textureScrollOffsetPerTick;
        public float explosionRadius;
        public SoundDef sustainerSoundDef;
        public int sustainerTickPeriod;
        public SoundDef trailSoundDef;
    }
}
