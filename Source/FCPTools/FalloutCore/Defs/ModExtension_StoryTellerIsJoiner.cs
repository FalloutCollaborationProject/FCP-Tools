namespace FCP.Core
{
    public class ModExtension_StoryTellerIsJoiner : DefModExtension
    {
        // Forced Defs
        public PawnKindDef pawnKindDef;
        public XenotypeDef forcedXenotypeDef;

        // Misc
        public bool onlyFixedPawnKindBackstories;
    
        // Faction
        public PawnFactionDefinition faction;
    
        // Appearance
        public PawnAppearanceDefinition appearance;

        // Name, Gender, Age
        public PawnStoryDefinition story;

        public IEnumerable<PawnGenerationDefinition> GetDefinitions()
        {
            if (faction != null) yield return faction;
            if (appearance != null) yield return appearance;
            if (story != null) yield return story;
        }
    }
}