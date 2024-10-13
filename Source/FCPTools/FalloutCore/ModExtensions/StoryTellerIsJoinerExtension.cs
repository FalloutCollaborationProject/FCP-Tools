using FCP.Core.Utils;
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
}

