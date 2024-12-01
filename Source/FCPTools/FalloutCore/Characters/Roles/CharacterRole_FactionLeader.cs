// ReSharper disable UnassignedField.Global
namespace FCP.Core;

[UsedImplicitly]
public class CharacterRole_FactionLeader : CharacterRole
{
    public int seniority;

    public override bool PawnIsValid(Pawn pawn)
    {
        if (pawn.Dead || pawn.IsPrisonerOfColony)
            return false;
        
        return base.PawnIsValid(pawn);
    }

    public override void ApplyRole(Pawn pawn)
    {
        base.ApplyRole(pawn);
        
        Faction faction = pawn.Faction;
        Ideo factionIdeo = faction.ideos.PrimaryIdeo;
        
        if (pawn.RaceProps.IsFlesh)
        {
            // I don't know what this does, the vanilla new leader method does this so...
            pawn.relations.everSeenByPlayer = true;
        }
        if (pawn.Ideo != factionIdeo)
        {
            pawn.ideo.SetIdeo(factionIdeo);
        }
        
        faction.leader = pawn;
    }

}