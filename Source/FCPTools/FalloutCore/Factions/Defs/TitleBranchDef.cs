namespace FCP.Factions;

[UsedImplicitly]
public class TitleBranchDef : Def
{
    public List<RoyalTitleDef> GetAwardableTitles(FactionDef factionDef, bool includeBranchless = true)
    {
        return factionDef.RoyalTitlesAwardableInSeniorityOrderForReading
            .Where(def =>
            {
                var ext = def.GetModExtension<TitleExtension_BranchTitle>();

                if (ext == null) return includeBranchless;
                return ext.branchDef == this;
            }).ToList();
    }
}