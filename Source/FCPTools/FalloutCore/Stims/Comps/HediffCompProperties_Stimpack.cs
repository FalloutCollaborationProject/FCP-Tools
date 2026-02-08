// ReSharper disable InconsistentNaming
namespace FCP.Core.Stims;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class HediffCompProperties_Stimpack : HediffCompProperties
{
    public bool HealPermanentInjuries = false;
    public bool HealMissingBodyParts = false;
    public bool RemoveHediffWhenDone = true;
    public bool ScaleHealedAmountWithTotalHitPoints = true;
    public float HealedAmount = 30f;
    public int TickMinimumBetweenHealing = 50;
    public int TickMaximumBetweenHealing = 100;
    public HediffCompProperties_Stimpack()
    {
        compClass = typeof(HediffComp_Stimpack);
    }
}