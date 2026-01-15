using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CompGiveThought
{
    public class CompGiveThought : ThingComp
    {
        public CompProperties_GiveThought Props => (CompProperties_GiveThought)props;

        public override void CompTick()
        {
            base.CompTick();
            if (parent.def?.tickerType != TickerType.Normal) return;
            // performance, u know...
            if ((Find.TickManager.TicksGame + parent.thingIDNumber) % 60 != 0) return;
            ApplyThought();
        }

        public override void CompTickRare()
        {
            base.CompTickRare();
            if (parent.def?.tickerType != TickerType.Rare) return;
            ApplyThought();
        }

        public override void CompTickLong()
        {
            base.CompTickLong();
            if (parent.def?.tickerType != TickerType.Long) return;
            ApplyThought();
        }

        protected void ApplyThought()
        {
            if (parent is Building_Bed bed)
            {
                if (bed?.CurOccupants == null) return;
                foreach (var occ in bed.CurOccupants) occ.needs.mood.thoughts.memories.TryGainMemory(Props.thoughtDef);
            }
            else if (parent is ThingWithComps thing)
            {
                if (thing.holdingOwner != null
                    && thing.holdingOwner.Owner is Pawn_EquipmentTracker equipment
                    && equipment?.pawn != null
                    && !equipment.pawn.Dead)
                    equipment.pawn.needs.mood.thoughts.memories.TryGainMemory(Props.thoughtDef);
                else if (thing.holdingOwner != null
                    && thing.holdingOwner.Owner is Pawn_ApparelTracker apparel
                    && apparel?.pawn != null
                    && !apparel.pawn.Dead)
                    apparel.pawn.needs.mood.thoughts.memories.TryGainMemory(Props.thoughtDef);
                else if (thing.holdingOwner != null
                   && thing.holdingOwner.Owner is Pawn_InventoryTracker inventory
                   && Props.enableInInventory
                   && inventory?.pawn != null
                   && !inventory.pawn.Dead)
                    inventory.pawn.needs.mood.thoughts.memories.TryGainMemory(Props.thoughtDef);
                else
                {
                    if (parent.Map == null || Props.radius == 0) return;
                    var cells = GenRadial.RadialCellsAround(parent.Position, Props.radius, true).Where(x => x.InBounds(parent.Map));
                    foreach (var cell in cells)
                    {
                        var pawn = cell.GetFirstPawn(parent.Map);
                        if (pawn == null) continue;
                        pawn.needs.mood.thoughts.memories.TryGainMemory(Props.thoughtDef);
                    }
                }
            }
        }

    }
}
