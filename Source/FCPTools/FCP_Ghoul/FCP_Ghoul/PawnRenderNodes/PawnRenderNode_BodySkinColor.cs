using UnityEngine;
using Verse;

namespace FCP_Ghoul
{
    public class PawnRenderNode_BodySkinColor : PawnRenderNode_Fur
    {
        public PawnRenderNode_BodySkinColor(Pawn pawn, PawnRenderNodeProperties props, 
            PawnRenderTree tree)
            : base(pawn, props, tree)
        {
            
        }
        
        public override Color ColorFor(Pawn pawn)
        {
            if (!pawn.genes.HasActiveGene(FCPGDefOf.FCP_Ghoul_SkinColor))
            {
                return Color.white;
            }
            
            Gene ghoulSkinGene = pawn.genes.GetGene(FCPGDefOf.FCP_Ghoul_SkinColor);
            if (ghoulSkinGene is Gene_GhoulSkin geneSkin)
            {
                Color bodyColor = geneSkin.SkinColor;
                return bodyColor;
            }
            return Color.white;
        }
    }
}