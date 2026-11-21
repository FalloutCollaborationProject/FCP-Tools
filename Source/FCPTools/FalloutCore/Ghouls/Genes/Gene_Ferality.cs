using UnityEngine;

namespace FCP.Core.Ghouls;

public class Gene_Ferality : Gene
{
    private float feralityValue;
    private int tickCounter;
    private bool needsTransform;
    private bool wentFeral;

    public float Ferality => feralityValue;
    public float FeralityPercent => feralityValue / 100f;

    public override void PostAdd()
    {
        base.PostAdd();
        var ext = def.GetModExtension<FeralityGene_ModExtension>();
        feralityValue = ext?.initialFerality ?? 0f;
        needsTransform = feralityValue >= 100f;
    }

    public override void Tick()
    {
        base.Tick();

        if (wentFeral)
            return;

        if (needsTransform)
        {
            needsTransform = false;
            TryGoFeral();
            return;
        }

        if (!pawn.IsHashIntervalTick(250)) return;

        var ext = def.GetModExtension<FeralityGene_ModExtension>();
        if (ext == null) return;

        tickCounter += 250;
        if (tickCounter >= ext.increaseIntervalTicks)
        {
            tickCounter = 0;
            feralityValue += ext.feralityIncreaseAmount;

            if (feralityValue >= 100f)
            {
                feralityValue = 100f;
                TryGoFeral();
            }
        }
    }

    public void ReduceFerality(float amount)
    {
        feralityValue -= amount;
        if (feralityValue < 0f)
            feralityValue = 0f;
    }

    public void SetFerality(float amount)
    {
        feralityValue = Mathf.Clamp(amount, 0f, 100f);
        needsTransform = feralityValue >= 100f;
    }

    private void TryGoFeral()
    {
        if (pawn.Dead || pawn.Downed) return;

        wentFeral = true;

        if (pawn.equipment?.Primary != null)
        {
            var eq = pawn.equipment.Primary;
            if (pawn.equipment.TryDropEquipment(eq, out ThingWithComps result, pawn.Position))
                result?.SetForbidden(false, false);
        }

        if (pawn.genes != null)
        {
            Gene skinGene = null;
            bool isGlowing = false;

            foreach (var gene in pawn.genes.GenesListForReading)
            {
                if (gene.def == FCPDefOf.FCP_Gene_Ghoul_Skin_GlowingOne)
                {
                    isGlowing = true;
                    break;
                }
                if (gene is Gene_GhoulBody)
                    skinGene = gene;
            }

            if (skinGene != null && !isGlowing)
            {
                pawn.genes.RemoveGene(skinGene);
                pawn.genes.AddGene(FCPDefOf.FCP_Gene_Ghoul_Feral_Skin, false);
                pawn.Drawer.renderer.SetAllGraphicsDirty();
                PortraitsCache.SetDirty(pawn);
            }
        }

        if (pawn.Faction?.IsPlayer == true)
            pawn.SetFaction(null);

        pawn.mindState.mentalStateHandler.TryStartMentalState(FCPDefOf.FCP_MentalState_PermanentBerserk, forceWake: true);

        Messages.Message("MessageFeralGhoul".Translate(pawn.Named("PAWN")), pawn, MessageTypeDefOf.NegativeEvent);
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        if (pawn?.Faction == Faction.OfPlayer)
            yield return new GeneGizmo_Ferality(this);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref feralityValue, "feralityValue", 0f);
        Scribe_Values.Look(ref tickCounter, "tickCounter", 0);
        Scribe_Values.Look(ref needsTransform, "needsTransform", false);
        Scribe_Values.Look(ref wentFeral, "wentFeral", false);
    }
}

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class FeralityGene_ModExtension : DefModExtension
{
    public int increaseIntervalTicks = 60000;
    public float feralityIncreaseAmount = 1f;
    public float amountReducedPerDrug = 20f;
    public float initialFerality = 0f;
    public List<ThingDef> drugs = new List<ThingDef>();
}