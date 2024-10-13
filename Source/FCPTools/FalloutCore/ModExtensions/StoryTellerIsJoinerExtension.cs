using System.Collections.Generic;
using FCP.Core.PawnGen;
using JetBrains.Annotations;
using RimWorld;
using Verse;
// ReSharper disable UnassignedField.Global

namespace FCP.Core;

[UsedImplicitly]
public class StoryTellerIsJoinerExtension : DefModExtension
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

