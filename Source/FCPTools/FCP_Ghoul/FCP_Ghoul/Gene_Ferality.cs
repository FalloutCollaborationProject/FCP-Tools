using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace FCP_Ghoul
{
    public class Gene_Ferality : Gene
    {
        public int quantity=0;
        public int rate=60;
        [Unsaved(false)]
        private GeneGizmo_Ferality gizmo;
        public Color customSkinColor;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref quantity, "quantity", forceSave:true);
            Scribe_Values.Look(ref rate, "rate", forceSave: true);
        }
        public override void PostAdd()
        {
            base.PostAdd();
            rate = this.def.GetModExtension<FeralityGene_ModExtension>().rate;
            quantity = 0;

        }
        public override void Tick()
        {
            base.Tick();
            if (quantity == 100)
            {
                PawnBanishUtility.Banish(pawn);
                pawn.mindState.mentalStateHandler.TryStartMentalState(Ghoul_Cache.PermanentBerserk, null, forced: false, forceWake: true);
                quantity = 101;
                pawn.genes.AddGene(Ghoul_Cache.FeralFur, true);
                pawn.genes.AddGene(Ghoul_Cache.FeralHead, true);
                //
                Color color = getColor();
                FeralHead_Gene headgene = (pawn.genes.GetGene(Ghoul_Cache.FeralHead) as FeralHead_Gene);
                headgene.r = color.r;
                headgene.g = color.g;
                headgene.b = color.b;
                pawn.Drawer.renderer.SetAllGraphicsDirty();
                Gene_FeralBody bodygene = (pawn.genes.GetGene(Ghoul_Cache.FeralFur) as Gene_FeralBody);
                bodygene.r = color.r;
                bodygene.g = color.g;
                bodygene.b = color.b;
                pawn.genes.RemoveGene(pawn.genes.GetGene(Ghoul_Cache.Fur));
                pawn.genes.RemoveGene(pawn.genes.GetGene(Ghoul_Cache.Head));
                pawn.genes.RemoveGene(pawn.genes.GetGene(Ghoul_Cache.SkinA));
                pawn.genes.RemoveGene(pawn.genes.GetGene(Ghoul_Cache.SkinB));
                pawn.genes.RemoveGene(pawn.genes.GetGene(Ghoul_Cache.SkinC));
                pawn.genes.RemoveGene(pawn.genes.GetGene(Ghoul_Cache.SkinD));
                pawn.genes.RemoveGene(pawn.genes.GetGene(Ghoul_Cache.SkinE));

                if (pawn.equipment.Primary != null)
                {
                    pawn.equipment.TryDropEquipment(pawn.equipment.Primary, out var _d, pawn.Position);
                }
                if (pawn.apparel.AnyApparel)
                {
                    if (Rand.Chance((def.GetModExtension<FeralityGene_ModExtension>().dropApparelChance)))
                    {
                        pawn.apparel.DropAll(pawn.Position);
                    }
                }
                
                pawn.genes.AddGene(Ghoul_Cache.SkinFeral, true);
                pawn.Drawer.renderer.SetAllGraphicsDirty();
                return;
            }
            if (pawn.IsHashIntervalTick(rate))
            {
                if (quantity<100)
                {
                    quantity++;
                }
                
            }
        }
        private Color getColor()
        {
            if (pawn.genes.GetGene(Ghoul_Cache.SkinA).Active)
            {
                return ((Color)pawn.genes.GetGene(Ghoul_Cache.SkinA).def.skinColorOverride);
            }
            if (pawn.genes.GetGene(Ghoul_Cache.SkinB).Active)
            {
                return ((Color)pawn.genes.GetGene(Ghoul_Cache.SkinB).def.skinColorOverride);
            }
            if (pawn.genes.GetGene(Ghoul_Cache.SkinC).Active)
            {
                return ((Color)pawn.genes.GetGene(Ghoul_Cache.SkinC).def.skinColorOverride);
            }
            if (pawn.genes.GetGene(Ghoul_Cache.SkinD).Active)
            {
                return ((Color)pawn.genes.GetGene(Ghoul_Cache.SkinD).def.skinColorOverride);
            }
            if (pawn.genes.GetGene(Ghoul_Cache.SkinE).Active)
            {
                return ((Color)pawn.genes.GetGene(Ghoul_Cache.SkinE).def.skinColorOverride);
            }
            return (Color.green);
        }
        public override void Notify_IngestedThing(Thing thing, int numTaken)
        {
            base.Notify_IngestedThing(thing, numTaken);
            if (def.GetModExtension<FeralityGene_ModExtension>().drugs.Contains(thing.def))
            {
                int reduce = def.GetModExtension<FeralityGene_ModExtension>().amountReduced;
                quantity -= reduce;
                if (quantity <= 0)
                {
                    quantity = 0;
                }
            }
        }
        public override IEnumerable<Gizmo> GetGizmos()
		{
			if (!Active||pawn.Faction!=Faction.OfPlayer)
			{
				yield break;
			}
			if (gizmo == null)
			{
				gizmo = new GeneGizmo_Ferality(this);
			}
			if (Find.Selector.SelectedPawns.Count == 1)
			{
				yield return gizmo;
			}
		}
	}
}
