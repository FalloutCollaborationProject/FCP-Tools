namespace FCP.Core
{
    public class CompGiveThought : ThingComp
    {
        public CompProperties_GiveThought Props => (CompProperties_GiveThought)this.props;

        public override void CompTick()
        {
            base.CompTick();
            ThingDef def = this.parent.def;
            if (def == null || def.tickerType != TickerType.Normal || (Find.TickManager.TicksGame + this.parent.thingIDNumber) % 60 != 0)
                return;
            this.ApplyThought();
        }

        public override void CompTickRare()
        {
            base.CompTickRare();
            ThingDef def = this.parent.def;
            if (def == null || def.tickerType != TickerType.Rare)
                return;
            this.ApplyThought();
        }

        public override void CompTickLong()
        {
            base.CompTickLong();
            ThingDef def = this.parent.def;
            if (def == null || def.tickerType != TickerType.Long)
                return;
            this.ApplyThought();
        }

        protected void ApplyThought()
        {
            if (this.parent is Building_Bed parent1)
            {
                if (parent1?.CurOccupants == null)
                    return;
                foreach (Pawn curOccupant in parent1.CurOccupants)
                    curOccupant.needs.mood.thoughts.memories.TryGainMemory(this.Props.thoughtDef);
            }
            else
            {
                ThingWithComps parent = this.parent;
                if (parent == null)
                    return;
                if (parent.holdingOwner != null && parent.holdingOwner.Owner is Pawn_EquipmentTracker owner3 && owner3?.pawn != null && !owner3.pawn.Dead)
                    owner3.pawn.needs.mood.thoughts.memories.TryGainMemory(this.Props.thoughtDef);
                else if (parent.holdingOwner != null && parent.holdingOwner.Owner is Pawn_ApparelTracker owner2 && owner2?.pawn != null && !owner2.pawn.Dead)
                    owner2.pawn.needs.mood.thoughts.memories.TryGainMemory(this.Props.thoughtDef);
                else if (parent.holdingOwner != null && parent.holdingOwner.Owner is Pawn_InventoryTracker owner1 && this.Props.enableInInventory && owner1?.pawn != null && !owner1.pawn.Dead)
                {
                    owner1.pawn.needs.mood.thoughts.memories.TryGainMemory(this.Props.thoughtDef);
                }
                else
                {
                    if (this.parent.Map == null || this.Props.radius == 0)
                        return;
                    foreach (IntVec3 c in GenRadial.RadialCellsAround(this.parent.Position, (float)this.Props.radius, true).Where<IntVec3>((Func<IntVec3, bool>)(x => x.InBounds(this.parent.Map))))
                        c.GetFirstPawn(this.parent.Map)?.needs.mood.thoughts.memories.TryGainMemory(this.Props.thoughtDef);
                }
            }
        }
    }
}
