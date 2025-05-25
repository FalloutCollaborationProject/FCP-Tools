using UnityEngine;
using Verse;

namespace FCP_Ghoul
{
    public class PawnRenderNode_HeadSkinColor : PawnRenderNode_Head
    {
        public PawnRenderNode_HeadSkinColor(Pawn pawn,PawnRenderNodeProperties props, 
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
                Color headColor = geneSkin.SkinColor;
                return headColor;
            }
            return Color.white;
        }
    }
}