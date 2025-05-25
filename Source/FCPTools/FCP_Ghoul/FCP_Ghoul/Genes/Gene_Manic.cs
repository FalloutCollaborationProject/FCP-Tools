using System.Collections.Generic;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;

namespace FCP_Ghoul
{
    [UsedImplicitly]
    public class Gene_Manic : Gene
    {
        private int _quantity = 0;
        private Color _fallbackSkinColor = Color.green;
        private ModExtension_Gene_Manic _modExt;
        private GeneGizmo_Ferality _gizmo;
        
        public int CurQuantity => _quantity;
        
        public override void PostAdd()
        {
            base.PostAdd();
            _modExt = def.GetModExtension<ModExtension_Gene_Manic>();
        }

        public override void Tick()
        {
            base.Tick();
            if (_quantity == _modExt.turnFeralThreshold)
            {
                TryTriggerFeralState();
                return;
            }
            
            if (!pawn.IsHashIntervalTick(_modExt.rate)) return;
            if (_quantity < _modExt.turnFeralThreshold)
            {
                _quantity++;
            }
        }
        
        private void TryTriggerFeralState()
        {
            PawnBanishUtility.Banish(pawn);
            
            pawn?.mindState?.mentalStateHandler?.TryStartMentalState(
                FCPGDefOf.FCP_MentalState_PermanentBerserk, forced: false, forceWake: true);
            
            // pawn?.genes?.AddGene(FCPGDefOf.FCP_Gene_Feral_Body, true);
            // pawn?.genes?.AddGene(FCPGDefOf.FCP_Gene_Feral_Head, true);
            
            // TODO: do in genes, not here, or set in mod extension???
            // Color color = GetColor();
            // if (pawn?.genes?.GetGene(FCPGDefOf.FCP_Gene_Feral_Head) is Gene_FeralHead headGene)
            // {
            //     headGene.headColor = color;
            // }
            // if (pawn?.genes?.GetGene(FCPGDefOf.FCP_Gene_Feral_Body) is Gene_FeralBody bodyGene)
            // {
            //     bodyGene.bodyColor = color;
            // }
            
            foreach (GeneDef geneDef in GeneCollections.Ghoul_TurnFeralGenesToRemove)
            {
                Gene gene = pawn?.genes?.GetGene(geneDef);
                if (gene != null)
                {
                    pawn?.genes?.RemoveGene(gene);
                }
            }
            
            if (pawn?.equipment?.Primary != null)
            {
                pawn?.equipment?.TryDropEquipment(pawn.equipment.Primary, out _, pawn.Position);
            }
            if (pawn?.apparel?.AnyApparel == true && Rand.Chance(_modExt?.dropApparelChance ?? 1f))
            {
                pawn?.apparel.DropAll(pawn.Position);
            }
            
            pawn?.genes?.AddGene(FCPGDefOf.FCP_Skin_Ghoul_Feral, true);
            pawn?.Drawer?.renderer?.SetAllGraphicsDirty();
        }
        
        // TODO: necessary???
        /*private Color GetColor()
        {
            foreach (GeneDef geneDef in GeneCollections.Ghoul_SkinGenes)
            {
                Gene gene = pawn?.genes?.GetGene(geneDef);
                if (gene is { Active: true } && gene.def.skinColorOverride.HasValue)
                {
                    return gene.def.skinColorOverride.Value;
                }
            }
            return _fallbackSkinColor;
        }*/
        
        public override void Notify_IngestedThing(Thing thing, int numTaken)
        {
            base.Notify_IngestedThing(thing, numTaken);
            
            if (_modExt?.drugs == null || !_modExt.drugs.Contains(thing.def)) return;
            _quantity -= _modExt.amountReduced;
            
            if (_quantity < 0)
            {
                _quantity = 0;
            }
        }
        
        public override IEnumerable<Gizmo> GetGizmos()
        {
            if (!Active || pawn?.Faction != Faction.OfPlayer)
                yield break;

            _gizmo ??= new GeneGizmo_Ferality(this);
            if (Find.Selector.SelectedPawns.Count == 1)
            {
                yield return _gizmo;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref _quantity, "_quantity");
        }
    }
}