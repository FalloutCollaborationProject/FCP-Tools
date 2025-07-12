using UnityEngine;

namespace FCP.Core.VATS;

[StaticConstructorOnStartup]
public class Command_VATS : Command_Ability
{
    public static readonly Texture2D Tex = ContentFinder<Texture2D>.Get("UI/FCP_VATS_Logo_Small");

    public Command_VATS(Ability ability, Pawn pawn)
        : base(ability, pawn)
    {
        icon = Tex;
        Verb.Ability = Ability;
    }

    public Verb_AbilityVATS Verb => (Verb_AbilityVATS)Ability.verb;

    public override bool Visible
    {
        get
        {
            if (Verb?.PrimaryWeaponVerbProps == null)
            {
                return false;
            }

            return !Verb.PrimaryWeaponVerbProps.IsMeleeAttack;
        }
    }

    public override Color IconDrawColor => defaultIconColor;

    public override void GizmoUpdateOnMouseover()
    {
        Verb.verbProps.DrawRadiusRing(Verb.caster.Position, Verb);
    }
}
