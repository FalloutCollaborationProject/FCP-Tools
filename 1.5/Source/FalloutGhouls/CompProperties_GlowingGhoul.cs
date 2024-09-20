using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace FalloutCore
{
    public class CompProperties_GlowingGhoul : CompProperties_Glower
    {
        public float toxicBuildupSeverityAdjust;
        public int toxicBuildupEmitTickRate;
        public float toxicBuildupEmitRadius;
        public CompProperties_GlowingGhoul()
        {
            this.compClass = typeof(Comp_GlowingGhoul);
        }

    }
}

