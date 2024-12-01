// ReSharper disable UnassignedField.Global

namespace FCP.Core;

[UsedImplicitly]
public class CharacterTitleDefinition : CharacterBaseDefinition
{
    // Todo make it able to give titles from factions other than the main one.
    public RoyalTitleDef title;

    public override bool AppliesPreGeneration => true;
    public override bool AppliesPostGeneration => title != null;

    public override void ApplyToRequest(ref PawnGenerationRequest request)
    {
        if (request.Faction != null && request.Faction.def.RoyalTitlesAllInSeniorityOrderForReading.Contains(title))
        {
            request.FixedTitle = title ?? request.FixedTitle;
            return;
        }

        FCPLog.Warning($"CharacterTitleDefinition had title {title.LabelCap} ({title.defName}) set, but the faction was invalid, Faction: {request.Faction?.Name ?? "null"}");
    }

    public override void ApplyToPawn(Pawn pawn)
    {
        pawn.royalty?.AllFactionPermits?.Clear();
    }
}