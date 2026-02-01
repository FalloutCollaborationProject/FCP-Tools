using RimWorld;
using System.Linq;
using Verse;

namespace FCP.Core.Ghouls
{
    public class Gene_Ghoul_GlowingOne : Gene
    {
        private int tickCounter;
        private int moteCounter;
        private const int RadiationTickInterval = 600;
        private const int MoteTickInterval = 120;
        private const float RadiationRadius = 3f;
        private const float ToxicBuildupAmount = 0.025f;
        private const float HealAmount = 3f;

        private static ThingDef moteDef;

        public override void PostAdd()
        {
            base.PostAdd();
            
            if (!pawn.health.hediffSet.HasHediff(HediffDefOf_Ghoul.ToxicHealing))
            {
                pawn.health.AddHediff(HediffDefOf_Ghoul.ToxicHealing);
            }
            
            if (moteDef == null)
            {
                moteDef = DefDatabase<ThingDef>.GetNamedSilentFail("FCP_Mote_GlowingOneAura");
            }
        }

        public override void PostRemove()
        {
            base.PostRemove();
            
            Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf_Ghoul.ToxicHealing);
            if (hediff != null)
            {
                pawn.health.RemoveHediff(hediff);
            }
        }

        public override void Tick()
        {
            base.Tick();

            if (!pawn.Spawned || pawn.Dead)
                return;

            tickCounter++;
            if (tickCounter >= RadiationTickInterval)
            {
                tickCounter = 0;
                EmitRadiation();
            }

            moteCounter++;
            if (moteCounter >= MoteTickInterval && moteDef != null)
            {
                moteCounter = 0;
                SpawnGlowMote();
            }
        }

        private void SpawnGlowMote()
        {
            if (pawn.Map == null)
                return;

            MoteThrown mote = (MoteThrown)ThingMaker.MakeThing(moteDef);
            mote.Scale = 2.5f;
            mote.rotationRate = 0.01f;
            mote.exactPosition = pawn.DrawPos;
            mote.SetVelocity(0, 0);
            GenSpawn.Spawn(mote, pawn.Position, pawn.Map);
        }

        private void EmitRadiation()
        {
            if (pawn.Map == null)
                return;

            foreach (Thing thing in GenRadial.RadialDistinctThingsAround(pawn.Position, pawn.Map, RadiationRadius, true))
            {
                Pawn target = thing as Pawn;
                if (target == null || target == pawn || target.Dead)
                    continue;

                bool isGhoul = target.genes?.GetFirstGeneOfType<Gene_GhoulBody>() != null;
                bool isGlowingOne = target.genes?.GetFirstGeneOfType<Gene_Ghoul_GlowingOne>() != null;
                
                if (isGhoul || isGlowingOne)
                {
                    Hediff_Injury injury = target.health.hediffSet.hediffs.OfType<Hediff_Injury>().FirstOrDefault(h => h.CanHealNaturally());
                    if (injury != null)
                        injury.Heal(HealAmount);
                }
                else
                {
                    Hediff toxicBuildup = target.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.ToxicBuildup);
                    if (toxicBuildup != null)
                    {
                        toxicBuildup.Severity += ToxicBuildupAmount;
                    }
                    else
                    {
                        toxicBuildup = target.health.AddHediff(HediffDefOf.ToxicBuildup);
                        toxicBuildup.Severity = ToxicBuildupAmount;
                    }
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref tickCounter, "tickCounter", 0);
        }
    }
}
