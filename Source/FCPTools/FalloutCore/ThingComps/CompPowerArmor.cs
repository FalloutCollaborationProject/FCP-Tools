// Decompiled with JetBrains decompiler
// Type: RangerRick_PowerArmor.CompPowerArmor
// Assembly: RangerRick_PowerArmor, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2F9A23E8-63EA-4090-93FB-DECA3711786E
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\RimWorld\Mods\FCP-Tools\1.5\Assemblies\RangerRick_PowerArmor.dll


using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace FCP.Core
{
    [HotSwappable]
    public class CompPowerArmor : ThingComp
    {
        private CompRefuelable _compRefuelable;

        public CompProperties_PowerArmor Props => props as CompProperties_PowerArmor;

        public CompRefuelable CompRefuelable
        {
            get => _compRefuelable ?? (_compRefuelable = parent.GetComp<CompRefuelable>());
        }

        public bool HasRequiredApparel(Pawn pawn)
        {
            return pawn.apparel.WornApparel.Any(y => Props.requiredApparels.Contains(y.def));
        }

        public bool HasRequiredTrait(Pawn pawn)
        {
            return pawn.story.traits.GetTrait(Props.requiredTrait) != null;
        }

        public override void CompTick()
        {
            base.CompTick();
            if (!(this.parent is Apparel parent) || parent.Wearer == null || Props.hediffOnEmptyFuel == null || CompRefuelable.HasFuel || parent.Wearer.health.hediffSet.GetFirstHediffOfDef(Props.hediffOnEmptyFuel) != null)
                return;
            parent.Wearer.health.AddHediff(Props.hediffOnEmptyFuel);
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);
            if (Props.hediffOnEmptyFuel == null)
                return;
            Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediffOnEmptyFuel);
            if (firstHediffOfDef == null)
                return;
            pawn.health.RemoveHediff(firstHediffOfDef);
        }

        public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
        {
            CompPowerArmor compPowerArmor = this;
            if (compPowerArmor.parent is Apparel parent)
            {
                CompRefuelable comp = compPowerArmor.parent.GetComp<CompRefuelable>();
                if (comp.Props.showFuelGizmo && Find.Selector.SingleSelectedThing == parent.Wearer)
                    yield return new Gizmo_RefuelableFuelStatus()
                    {
                        refuelable = comp
                    };
                foreach (Gizmo gizmo in comp.CompGetGizmosExtra())
                    yield return gizmo;
                comp = null;
            }
        }
    }
}
