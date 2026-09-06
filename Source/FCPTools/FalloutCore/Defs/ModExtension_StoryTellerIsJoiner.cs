namespace FCP.Core;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class ModExtension_StoryTellerIsJoiner : DefModExtension
{
    public CharacterDef characterDef;
    public List<CharacterDef> characterDefs;

    public IEnumerable<CharacterDef> AllCharacterDefs
    {
        get
        {
            if (characterDef != null)
                yield return characterDef;
            if (characterDefs != null)
                foreach (CharacterDef def in characterDefs)
                    yield return def;
        }
    }
}