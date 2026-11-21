using RimWorld;
using RimWorld.Planet;
using Verse;

namespace FCP.Core.Ghouls;

public class GenStep_FeralGhoulNest : GenStep
{
    public PawnKindDef pawnKindDef;
    public int minGhouls = 8;
    public int maxGhouls = 16;

    public override int SeedPart => 982347123;

    public override void Generate(Map map, GenStepParams parms)
    {
        if (!ModsConfig.BiotechActive || pawnKindDef == null) return;

        int count = Rand.RangeInclusive(minGhouls, maxGhouls);
        IntVec3 center = map.Center;

        for (int i = 0; i < count; i++)
        {
            if (!CellFinder.TryFindRandomCellNear(center, map, 30, c => c.Standable(map) && !c.Fogged(map), out IntVec3 loc))
                continue;

            Pawn ghoul = PawnGenerator.GeneratePawn(pawnKindDef, null);
            EnsureFeralityGene(ghoul);
            GenSpawn.Spawn(ghoul, loc, map);

            ghoul.mindState.mentalStateHandler.TryStartMentalState(FCPDefOf.FCP_MentalState_PermanentBerserk, forceWake: true);
        }
    }

    private void EnsureFeralityGene(Pawn pawn)
    {
        var gene = pawn.genes?.GetFirstGeneOfType<Gene_Ferality>();
        if (gene == null && pawn.genes != null)
        {
            pawn.genes.AddGene(FCPDefOf.FCP_Gene_Ferality, false);
            gene = pawn.genes.GetFirstGeneOfType<Gene_Ferality>();
        }
        gene?.SetFerality(100f);
    }
}

public class GenStep_GlowingOne : GenStep
{
    public PawnKindDef pawnKindDef;
    public bool glowingOneIsBerserk;

    public override int SeedPart => 723491823;

    public override void Generate(Map map, GenStepParams parms)
    {
        if (!ModsConfig.BiotechActive || pawnKindDef == null) return;

        if (!CellFinder.TryFindRandomCellNear(map.Center, map, 15, c => c.Standable(map) && !c.Fogged(map), out IntVec3 loc))
            return;

        Pawn glowingOne = PawnGenerator.GeneratePawn(pawnKindDef, null);
        EnsureFeralityGene(glowingOne);
        GenSpawn.Spawn(glowingOne, loc, map);

        if (glowingOneIsBerserk)
            glowingOne.mindState.mentalStateHandler.TryStartMentalState(FCPDefOf.FCP_MentalState_PermanentBerserk, forceWake: true);

        SpawnGuards(loc, map);
    }

    private void SpawnGuards(IntVec3 center, Map map)
    {
        for (int i = 0; i < Rand.RangeInclusive(2, 4); i++)
        {
            if (!CellFinder.TryFindRandomCellNear(center, map, 8, c => c.Standable(map) && !c.Fogged(map), out IntVec3 guardLoc))
                continue;

            Pawn guard = PawnGenerator.GeneratePawn(FCPDefOf.FCP_Pawnkind_Ghoul_Feral, null);
            EnsureFeralityGene(guard);
            GenSpawn.Spawn(guard, guardLoc, map);

            guard.mindState.mentalStateHandler.TryStartMentalState(FCPDefOf.FCP_MentalState_PermanentBerserk, forceWake: true);
        }
    }

    private void EnsureFeralityGene(Pawn pawn)
    {
        var gene = pawn.genes?.GetFirstGeneOfType<Gene_Ferality>();
        if (gene == null && pawn.genes != null)
        {
            pawn.genes.AddGene(FCPDefOf.FCP_Gene_Ferality, false);
            gene = pawn.genes.GetFirstGeneOfType<Gene_Ferality>();
        }
        gene?.SetFerality(100f);
    }
}