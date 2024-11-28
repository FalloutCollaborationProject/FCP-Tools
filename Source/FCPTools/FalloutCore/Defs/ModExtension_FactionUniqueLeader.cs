namespace FCP.Core;

/// <summary>
/// Defines a faction as having a unique leader
/// </summary>
public class ModExtension_FactionUniqueLeader : DefModExtension
{
    public List<CharacterDef> characterDefs;

    public override IEnumerable<string> ConfigErrors()
    {
        if (characterDefs == null || characterDefs.Count == 0)
        {
            yield return "A Faction has a ModExtension_FactionUniqueLeader but has no characters listed";
        }
    }
}