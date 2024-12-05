using RimWorld;
using Verse;

namespace ArmorRacks.Things
{
    public class MendingArmorRack : MechanizedArmorRack
    {
        public int TickCounter = 0;
        private int? RareTicksPerMendCached;

        public int RareTicksPerMend
        {
            get
            {
                if (RareTicksPerMendCached == null)
                {
                    RareTicksPerMendCached = LoadedModManager.GetMod<ArmorRacksMod>().GetSettings<ArmorRacksModSettings>().RareTicksPerMend;
                }
                return (int) RareTicksPerMendCached;
            }
        }
        public override void TickRare()
        {
            var power = GetComp<CompPowerTrader>();
            if (power.PowerOn)
            {
                TickCounter++;
            }
            if (TickCounter >= RareTicksPerMend)
            {
                MendContents();
                TickCounter = 0;
            }
            base.TickRare();
        }

        public void MendContents()
        {
            foreach (Thing thing in InnerContainer.InnerListForReading)
            {
                if (thing.HitPoints < thing.MaxHitPoints)
                {
                    thing.HitPoints++;
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref TickCounter, "ArmorRackTickCounter");
        }
        
    }
}