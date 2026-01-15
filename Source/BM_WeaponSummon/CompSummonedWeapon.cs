using RimWorld;
using System.Linq;
using Verse;

namespace BM_WeaponSummon
{
    public class CompProperties_SummonedWeapon : CompProperties
    {
        public int lifetimeDuration;
        public FleckDef fleckWhenExpired;
        public CompProperties_SummonedWeapon() => compClass = typeof(CompSummonedWeapon);
    }

    public class CompSummonedWeapon : ThingComp
    {
        public int ticksSummoned;
        private int lastTicked;
        public CompProperties_SummonedWeapon Props => base.props as CompProperties_SummonedWeapon;
        public override void CompTick()
        {
            base.CompTick();
            if (lastTicked != Find.TickManager.TicksGame)
            {
                lastTicked = Find.TickManager.TicksGame; 
                var owner = parent.ParentHolder as Pawn_EquipmentTracker;
                if (Find.TickManager.TicksGame - ticksSummoned >= Props.lifetimeDuration || owner is null)
                {
                    var mapHeld = parent.MapHeld;
                    if (mapHeld != null && Props.fleckWhenExpired != null)
                    {
                        FleckMaker.Static(parent.PositionHeld, mapHeld, Props.fleckWhenExpired);
                    }
                    parent.Destroy();
                    if (owner != null)
                    {
                        var existingWeapon = owner.pawn.inventory.innerContainer.Where(x => x.def.IsWeapon).FirstOrDefault() as ThingWithComps;
                        if (existingWeapon != null)
                        {
                            existingWeapon.holdingOwner?.Remove(existingWeapon);
                            owner.AddEquipment(existingWeapon);
                        }
                    }
                }
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref ticksSummoned, "ticksSummoned");
        }
    }
}
