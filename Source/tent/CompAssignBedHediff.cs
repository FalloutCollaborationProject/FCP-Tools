using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Tent
{
    class CompAssignBedHediff : ThingComp
    {
        public override void CompTick()
        {
            base.CompTick();
            if (parent.def?.tickerType != TickerType.Normal) return;
            if ((Find.TickManager.TicksGame + parent.thingIDNumber) % 60 != 0) return;
            ApplyHediff();
        }

        public override void CompTickRare()
        {
            base.CompTickRare();
            if (parent.def?.tickerType != TickerType.Rare) return;
            ApplyHediff();
        }

        public override void CompTickLong()
        {
            base.CompTickLong();
            if (parent.def?.tickerType != TickerType.Long) return;
            ApplyHediff();
        }

        private void ApplyHediff()
        {
            var bed = parent as Building_Bed;
            if (bed == null) return;
            var modExt = bed.def.GetModExtension<TentModExtension>();
            if (modExt?.customHediff == null) return;
            var refuelableComp = bed.TryGetComp<CompRefuelable>();
            if (refuelableComp != null && !refuelableComp.HasFuel) return;
            foreach (var sleepyhead in bed.CurOccupants) if (!sleepyhead.health.hediffSet.HasHediff(modExt.customHediff)) sleepyhead.health.AddHediff(modExt.customHediff);
        }

    }
}
