namespace FCP.Core.Ghouls;

public static class GhoulUtility
{
    public static bool IsGhoul(Pawn pawn)
    {
        List<Gene> genes = pawn?.genes?.GenesListForReading;
        if (genes == null)
            return false;

        for (int i = 0; i < genes.Count; i++)
        {
            if (genes[i].Active && genes[i].def.exclusionTags != null && genes[i].def.exclusionTags.Contains("GhoulSkin"))
                return true;
        }
        return false;
    }
}
