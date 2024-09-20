using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Noise;

namespace FalloutCore
{
    public class CompProperties_HiddenFeralGhoul : CompProperties
    {
        public SimpleCurve chanceToTurnInYears;
        public CompProperties_HiddenFeralGhoul()
        {
            this.compClass = typeof(CompHiddenFeralGhoul);
        }
    }

    public class CompHiddenFeralGhoul : ThingComp
    {
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad); 
            if (!respawningAfterLoad)
            {
                nextFeralChanceCheckTick = Find.TickManager.TicksGame + (GenDate.TicksPerDay * Rand.RangeInclusive(40, 80));
            }
        }
        public CompProperties_HiddenFeralGhoul Props => this.props as CompProperties_HiddenFeralGhoul;

        public int nextFeralChanceCheckTick;

        public Pawn Ghoul => this.parent as Pawn;
        public override void CompTick()
        {
            base.CompTick();
            DoCheck();
        }

        public void DoCheck()
        {
            var ghoul = Ghoul;
            if (!ghoul.IsFeralGhoul() && this.parent.Map != null && Find.TickManager.TicksGame > nextFeralChanceCheckTick)
            {
                var chance = Props.chanceToTurnInYears.Evaluate(ghoul.ageTracker.AgeBiologicalYearsFloat);
                if (Rand.Chance(chance))
                {
                    ConvertToFeral(ghoul);
                }
                nextFeralChanceCheckTick = Find.TickManager.TicksGame + (GenDate.TicksPerDay * Rand.RangeInclusive(40, 80));
            }
        }

        public void ConvertToFeral(Pawn pawn)
        {
            var pawnKindDefName = "Feral" + pawn.kindDef.defName;
            var pawnKindDef = DefDatabase<PawnKindDef>.GetNamedSilentFail(pawnKindDefName) ?? DefDatabase<PawnKindDef>.GetNamed("FeralGhoul_Pawn");
            var newPawn = PawnUtils.GetPawnDuplicate(pawn, pawnKindDef);
            GenSpawn.Spawn(newPawn, pawn.Position, pawn.Map);
            pawn.Destroy(DestroyMode.Vanish);
            newPawn.SetFaction(null);
            newPawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent);
            newPawn.equipment.DropAllEquipment(newPawn.Position);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref nextFeralChanceCheckTick, "nextFeralChanceCheckTick");
        }
    }
}