using RimWorld;
using RimWorld.BaseGen;
using Verse;
using System.Linq;

namespace FCP_Ghoul
{
    public class GenStep_FeralGhoulSpawn : GenStep
    {
        public PawnKindDef pawnKindDef;
        public float spawnChance = 0.35f;
        public int minPackSize = 2;
        public int maxPackSize = 5;

        public override int SeedPart => 674893246;

        public override void Generate(Map map, GenStepParams parms)
        {
            if (!ModsConfig.BiotechActive || pawnKindDef == null)
                return;

            if (Rand.Value > spawnChance)
                return;

            int packSize = Rand.RangeInclusive(minPackSize, maxPackSize);

            for (int i = 0; i < packSize; i++)
            {
                IntVec3 loc;
                if (!CellFinder.TryFindRandomEdgeCellWith(c => 
                    c.Standable(map) && 
                    !c.Fogged(map),
                    map, CellFinder.EdgeRoadChance_Ignore, out loc))
                {
                    continue;
                }

                Pawn ghoul = PawnGenerator.GeneratePawn(pawnKindDef, null);
                
                var feralityGene = ghoul.genes?.GetFirstGeneOfType<Gene_Ferality>();
                if (feralityGene != null)
                {
                    feralityGene.SetFerality(100f);
                }

                GenSpawn.Spawn(ghoul, loc, map);
                
                var mentalState = DefDatabase<MentalStateDef>.GetNamed("FCP_PermanentBerserk", false);
                if (mentalState != null)
                {
                    ghoul.mindState.mentalStateHandler.TryStartMentalState(mentalState, forceWake: true);
                }
            }
        }
    }
}
