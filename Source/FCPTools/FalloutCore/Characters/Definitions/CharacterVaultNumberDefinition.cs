namespace FCP.Core;

public class CharacterVaultNumberDefinition : CharacterBaseDefinition
{
    public string vaultNumber;

    public override bool AppliesPreGeneration => false;
    public override bool AppliesPostGeneration => true;

    public override void ApplyToPawn(Pawn pawn)
    {
        if (vaultNumber.NullOrEmpty() || pawn.apparel == null)
            return;

        foreach (Apparel apparel in pawn.apparel.WornApparel)
        {
            apparel.TryGetComp<CompVaultNumberStencil>()?.SetNumber(vaultNumber);
        }
    }
}
