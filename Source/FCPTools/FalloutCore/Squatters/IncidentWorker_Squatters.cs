namespace FCP.Core;

public class IncidentWorker_Squatters : IncidentWorker
{
    protected override bool CanFireNowSub(IncidentParms parms)
    {
        return parms.target is Map map && FindEmptyBed(map) != null;
    }

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        if (parms.target is not Map map)
            return false;

        Building_Bed bed = FindEmptyBed(map);
        if (bed == null)
            return false;

        if (!RCellFinder.TryFindRandomPawnEntryCell(out IntVec3 entryCell, map, CellFinder.EdgeRoadChance_Ignore))
            return false;

        int count = Rand.RangeInclusive(1, 2);
        List<Pawn> squatters = new List<Pawn>();
        for (int i = 0; i < count; i++)
        {
            PawnGenerationRequest request = new PawnGenerationRequest(PawnKindDefOf.Drifter, null, PawnGenerationContext.NonPlayer, map.Tile);
            Pawn pawn = PawnGenerator.GeneratePawn(request);
            GenSpawn.Spawn(pawn, CellFinder.RandomClosewalkCellNear(entryCell, map, 10), map);
            pawn.mindState.forcedGotoPosition = bed.Position;
            squatters.Add(pawn);
        }

        ChoiceLetter_Squatters letter = (ChoiceLetter_Squatters)LetterMaker.MakeLetter(
            def.letterLabel.Translate(),
            def.letterText.Translate(squatters.Count),
            FCPDefOf.FCP_Letter_Squatters,
            new LookTargets(squatters));
        letter.squatters = squatters;
        Find.LetterStack.ReceiveLetter(letter);

        return true;
    }

    private static Building_Bed FindEmptyBed(Map map)
    {
        return map.listerBuildings.AllBuildingsColonistOfClass<Building_Bed>()
            .FirstOrDefault(b => !b.ForPrisoners && !b.Medical && b.OwnersForReading.Count == 0);
    }
}
