﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace FalloutCore
{
    public class DeathEffectModExtension : DefModExtension
    {
        public float effectChance = 0f;

        public ThingDef effectSpawner;
    }
}

