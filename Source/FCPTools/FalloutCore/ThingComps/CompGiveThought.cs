namespace FCP.Core;

public class CompGiveThought : ThingComp
{
    private CompProperties_GiveThought Props => (CompProperties_GiveThought)props;

    public override void CompTick()
    {
        base.CompTick();
        if (parent.def?.tickerType != TickerType.Normal) return;
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

    private void ApplyThought()
    {
        switch (parent)
        {
            case Building_Bed bed:
            {
                if (bed.CurOccupants == null) return;
                foreach (Pawn pawn in bed.CurOccupants)
                    pawn.needs.mood.thoughts.memories.TryGainMemory(Props.thoughtDef);
                break;
            }
            case ThingWithComps thing:
            {
                switch (thing.holdingOwner?.Owner)
                {
                    // Equipment of an Alive Pawn
                    case Pawn_EquipmentTracker { pawn: { Dead: false } equipPawn }:
                        equipPawn.needs.mood.thoughts.memories.TryGainMemory(Props.thoughtDef);
                        break;
                    // Apparel of an Alive Pawn
                    case Pawn_ApparelTracker { pawn: { Dead: false } apparelPawn }:
                        apparelPawn.needs.mood.thoughts.memories.TryGainMemory(Props.thoughtDef);
                        break;
                    // Else it's probably an enable in inventory option or for a radius
                    default:
                    {
                        if (Props.enableInInventory && thing.holdingOwner?.Owner is Pawn_InventoryTracker { pawn: { Dead: false } invPawn })
                            invPawn.needs.mood.thoughts.memories.TryGainMemory(Props.thoughtDef);
                        else
                            ApplyThoughtInRadius();
                        break;
                    }
                }

                break;
            }
        }
    }

    private void ApplyThoughtInRadius()
    {
        if (parent.Map == null || Props.radius == 0) return;

        var cells = GenRadial.RadialCellsAround(parent.Position, Props.radius, true)
            .Where(cell => cell.InBounds(parent.Map));

        foreach (IntVec3 cell in cells)
        {
            Pawn pawn = cell.GetFirstPawn(parent.Map);
            pawn?.needs.mood.thoughts.memories.TryGainMemory(Props.thoughtDef);
        }
    }
}