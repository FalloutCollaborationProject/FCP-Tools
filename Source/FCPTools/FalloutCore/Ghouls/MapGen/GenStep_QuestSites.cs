using RimWorld;
using RimWorld.Planet;
using Verse;

namespace FCP.Core.Ghouls
{
    public class GenStep_FeralGhoulNest : GenStep
    {
        public PawnKindDef pawnKindDef;
        public int minGhouls = 8;
        public int maxGhouls = 16;

        private static GeneDef feralityGeneDef;
        private static MentalStateDef berserkDef;

        public override int SeedPart => 982347123;

        public override void Generate(Map map, GenStepParams parms)
        {
            if (!ModsConfig.BiotechActive || pawnKindDef == null) return;

            if (feralityGeneDef == null) feralityGeneDef = DefDatabase<GeneDef>.GetNamed("FCP_Gene_Ferality", false);
            if (berserkDef == null) berserkDef = DefDatabase<MentalStateDef>.GetNamed("FCP_PermanentBerserk", false);

            int count = Rand.RangeInclusive(minGhouls, maxGhouls);
            IntVec3 center = map.Center;

            for (int i = 0; i < count; i++)
            {
                if (!CellFinder.TryFindRandomCellNear(center, map, 30, c => c.Standable(map) && !c.Fogged(map), out IntVec3 loc)) 
                    continue;

                Pawn ghoul = PawnGenerator.GeneratePawn(pawnKindDef, null);
                EnsureFeralityGene(ghoul);
                GenSpawn.Spawn(ghoul, loc, map);
                
                if (berserkDef != null)
                    ghoul.mindState.mentalStateHandler.TryStartMentalState(berserkDef, forceWake: true);
            }
        }

        private void EnsureFeralityGene(Pawn pawn)
        {
            var gene = pawn.genes?.GetFirstGeneOfType<Gene_Ferality>();
            if (gene == null && feralityGeneDef != null && pawn.genes != null)
            {
                pawn.genes.AddGene(feralityGeneDef, false);
                gene = pawn.genes.GetFirstGeneOfType<Gene_Ferality>();
            }
            gene?.SetFerality(100f);
        }
    }

    public class GenStep_GlowingOne : GenStep
    {
        public PawnKindDef pawnKindDef;
        public bool glowingOneIsBerserk;

        private static GeneDef feralityGeneDef;
        private static MentalStateDef berserkDef;
        private static PawnKindDef feralKindDef;

        public override int SeedPart => 723491823;

        public override void Generate(Map map, GenStepParams parms)
        {
            if (!ModsConfig.BiotechActive || pawnKindDef == null) return;

            if (feralityGeneDef == null) feralityGeneDef = DefDatabase<GeneDef>.GetNamed("FCP_Gene_Ferality", false);
            if (berserkDef == null) berserkDef = DefDatabase<MentalStateDef>.GetNamed("FCP_PermanentBerserk", false);
            if (feralKindDef == null) feralKindDef = DefDatabase<PawnKindDef>.GetNamed("FCP_Pawnkind_Ghoul_Feral", false);

            if (!CellFinder.TryFindRandomCellNear(map.Center, map, 15, c => c.Standable(map) && !c.Fogged(map), out IntVec3 loc)) 
                return;

            Pawn glowingOne = PawnGenerator.GeneratePawn(pawnKindDef, null);
            EnsureFeralityGene(glowingOne);
            GenSpawn.Spawn(glowingOne, loc, map);
            
            if (glowingOneIsBerserk && berserkDef != null)
                glowingOne.mindState.mentalStateHandler.TryStartMentalState(berserkDef, forceWake: true);

            SpawnGuards(loc, map);
        }

        private void SpawnGuards(IntVec3 center, Map map)
        {
            if (feralKindDef == null) return;

            for (int i = 0; i < Rand.RangeInclusive(2, 4); i++)
            {
                if (!CellFinder.TryFindRandomCellNear(center, map, 8, c => c.Standable(map) && !c.Fogged(map), out IntVec3 guardLoc)) 
                    continue;

                Pawn guard = PawnGenerator.GeneratePawn(feralKindDef, null);
                EnsureFeralityGene(guard);
                GenSpawn.Spawn(guard, guardLoc, map);
                
                if (berserkDef != null)
                    guard.mindState.mentalStateHandler.TryStartMentalState(berserkDef, forceWake: true);
            }
        }

        private void EnsureFeralityGene(Pawn pawn)
        {
            var gene = pawn.genes?.GetFirstGeneOfType<Gene_Ferality>();
            if (gene == null && feralityGeneDef != null && pawn.genes != null)
            {
                pawn.genes.AddGene(feralityGeneDef, false);
                gene = pawn.genes.GetFirstGeneOfType<Gene_Ferality>();
            }
            gene?.SetFerality(100f);
        }
    }
}
