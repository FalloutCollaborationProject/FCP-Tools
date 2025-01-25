// Decompiled with JetBrains decompiler
// Type: RangerRick_PowerArmor.CompProperties_PowerArmor
// Assembly: RangerRick_PowerArmor, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2F9A23E8-63EA-4090-93FB-DECA3711786E
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\RimWorld\Mods\FCP-Tools\1.5\Assemblies\RangerRick_PowerArmor.dll

using RimWorld;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace FCP.Core
{
    public class CompProperties_PowerArmor : CompProperties
    {
        public List<ThingDef> requiredApparels;
        public TraitDef requiredTrait;
        public HediffDef hediffOnEmptyFuel;

        public CompProperties_PowerArmor() => this.compClass = typeof(CompPowerArmor);
    }
}
