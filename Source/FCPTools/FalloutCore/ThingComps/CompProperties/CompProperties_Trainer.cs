// Decompiled with JetBrains decompiler
// Type: RangerRick_PowerArmor.CompProperties_Trainer
// Assembly: RangerRick_PowerArmor, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2F9A23E8-63EA-4090-93FB-DECA3711786E
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\RimWorld\Mods\FCP-Tools\1.5\Assemblies\RangerRick_PowerArmor.dll

using RimWorld;
using Verse;

#nullable disable
namespace FCP.Core
{
    public class CompProperties_Trainer : CompProperties
    {
        public TraitDef givesTrait;
        public int trainDuration;

        public CompProperties_Trainer() => this.compClass = typeof(CompTrainer);
    }
}
