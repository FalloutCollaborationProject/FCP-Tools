using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RangerRick_PowerArmor
{

    public class CompProperties_PowerArmor : CompProperties
    {
        public List<ThingDef> requiredApparels;
        public TraitDef requiredTrait;
        public HediffDef hediffOnEmptyFuel;
        public List<WorkTypeDef> workDisables;
        public bool canSleep = true;
        public bool ignoresLegs = false;
        public CompProperties_PowerArmor()
        {
            this.compClass = typeof(CompPowerArmor);
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class HotSwappableAttribute : Attribute
    {
    }

    [HotSwappableAttribute]

    public class CompPowerArmor : ThingComp
    {
        public CompProperties_PowerArmor Props => base.props as CompProperties_PowerArmor;

        private CompRefuelable _compRefuelable;
        public CompRefuelable CompRefuelable => _compRefuelable ??= parent.GetComp<CompRefuelable>();

        public bool HasRequiredApparel(Pawn pawn)
        {
            return Props.requiredApparels is null || pawn.apparel.WornApparel.Any(y => Props.requiredApparels.Contains(y.def));
        }
        public bool HasRequiredTrait(Pawn pawn)
        {
            return Props.requiredTrait is null || pawn.story.traits.GetTrait(Props.requiredTrait) != null;
        }

        public override void CompTick()
        {
            base.CompTick();
            if (parent is Apparel apparel && apparel.Wearer is not null)
            {
                var comp = CompRefuelable;
                comp.ConsumeFuel(CompRefuelable.ConsumptionRatePerTick);
                if (Props.hediffOnEmptyFuel != null)
                {
                    if (comp.HasFuel is false)
                    {
                        var hediff = apparel.Wearer.health.hediffSet.GetFirstHediffOfDef(Props.hediffOnEmptyFuel);
                        if (hediff is null)
                        {
                            apparel.Wearer.health.AddHediff(Props.hediffOnEmptyFuel);
                        }
                    }
                }
            }
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);
            if (Props.hediffOnEmptyFuel != null)
            {
                var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediffOnEmptyFuel);
                if (hediff != null)
                {
                    pawn.health.RemoveHediff(hediff);
                }
            }
        }

        public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
        {
            if (parent is Apparel apparel)
            {
                var comp = parent.GetComp<CompRefuelable>();
                if (comp.Props.showFuelGizmo && Find.Selector.SingleSelectedThing == apparel.Wearer)
                {
                    var gizmo_RefuelableFuelStatus = new Gizmo_SetFuelLevel(comp);
                    yield return gizmo_RefuelableFuelStatus;
                }
                foreach (var g in comp.CompGetGizmosExtra())
                {
                    yield return g;
                }

            }
        }
    }
}
