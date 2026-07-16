using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace FCP.Enlist;

public class SalaryInfo : IExposable
{
    public int lastPaidTick;
    public bool CanPayMoney(FactionEnlistOptionsDef options)
    {
        return Find.TickManager.TicksGame >= lastPaidTick + (options.salaryPeriodDays * GenDate.TicksPerDay);
    }
    public void GiveMoney(FactionEnlistOptionsDef options, Caravan caravan, Faction faction)
    {
        float multiplier = 1f;
        if (options.titleWageBonuses != null && faction != null)
        {
            foreach (Pawn pawn in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists)
            {
                if (pawn.royalty == null) continue;
                foreach (TitleWageBonus wagebonus in options.titleWageBonuses)
                {
                    if (wagebonus.title == null) continue;
                    RoyalTitle currentTitle = pawn.royalty.GetCurrentTitleInFaction(faction);
                    if (currentTitle != null && currentTitle.def.seniority >= wagebonus.title.seniority)
                        multiplier = Mathf.Max(multiplier, wagebonus.multiplier);
                }
            }
        }
        int tickDiff = Find.TickManager.TicksGame - lastPaidTick;
        int salaryPeriodTicks = options.salaryPeriodDays * GenDate.TicksPerDay;
        while (tickDiff > salaryPeriodTicks)
        {
            float curBatch = options.salaryRange.RandomInRange * multiplier;
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