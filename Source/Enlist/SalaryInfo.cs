using RimWorld;
using RimWorld.Planet;
using Verse;

namespace  FCP.Enlist
{
    public class SalaryInfo : IExposable
    {
        public int lastPaidTick;
        public bool CanPayMoney(FactionEnlistOptionsDef options)
        {
            return Find.TickManager.TicksGame >= lastPaidTick + (options.salaryPeriodDays * GenDate.TicksPerDay);
        }
        public void GiveMoney(FactionEnlistOptionsDef options, Caravan caravan)
        {
            int tickDiff = Find.TickManager.TicksGame - lastPaidTick;
            int salaryPeriodTicks = options.salaryPeriodDays * GenDate.TicksPerDay;
            while (tickDiff > salaryPeriodTicks)
            {
                float curBatch = options.salaryRange.RandomInRange;
                Thing silver = ThingMaker.MakeThing(options.salaryDef);
                silver.stackCount = (int)curBatch;
                CaravanInventoryUtility.GiveThing(caravan, silver);
                tickDiff -= salaryPeriodTicks;
            }
            lastPaidTick = Find.TickManager.TicksGame;
        }
        public void ExposeData()
        {
            Scribe_Values.Look(ref lastPaidTick, "lastPaidTick");
        }
    }
}
