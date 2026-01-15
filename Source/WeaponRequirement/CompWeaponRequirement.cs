using System.Collections.Generic;
using System.Linq;
using Verse;

namespace WeaponRequirement
{
    public class CompProperties_WeaponRequirement : CompProperties
    {
        public List<ThingDef> requiredApparels;
        public HediffDef hediffWithMissingApparels;
        public CompProperties_WeaponRequirement()
        {
            this.compClass = typeof(CompWeaponRequirement);
        }
    }

    public class CompWeaponRequirement : ThingComp
    {
        public CompProperties_WeaponRequirement Props => base.props as CompProperties_WeaponRequirement;

        public bool HasRequiredApparel(Pawn pawn)
        {
            return pawn.apparel.WornApparel.Any(y => Props.requiredApparels.Contains(y.def));
        }

        private int lastTicked;
        public override void CompTick()
        {
            if (lastTicked != Find.TickManager.TicksGame)
            {
                lastTicked = Find.TickManager.TicksGame;
                base.CompTick();
                var owner = parent.ParentHolder as Pawn_EquipmentTracker;
                if (owner != null && Props.hediffWithMissingApparels != null)
                {
                    var hediff = owner.pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediffWithMissingApparels);
                    bool hasRequiredApparels = HasRequiredApparel(owner.pawn);
                    if (hediff is null && hasRequiredApparels is false)
                    {
                        owner.pawn.health.AddHediff(Props.hediffWithMissingApparels);
                    }
                    else if (hediff is not null && hasRequiredApparels)
                    {
                        owner.pawn.health.RemoveHediff(hediff);
                    }
                }
            }
        }
    }
}
