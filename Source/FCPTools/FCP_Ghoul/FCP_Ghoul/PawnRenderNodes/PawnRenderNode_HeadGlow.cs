using FCP.Core;
using UnityEngine;
using Verse;

namespace FCP_Ghoul
{
    public class PawnRenderNode_HeadGlow : PawnRenderNode
    {
        public PawnRenderNode_HeadGlow(Pawn pawn,PawnRenderNodeProperties props, 
            PawnRenderTree tree) 
            : base(pawn, props, tree)
        {
            gene = pawn?.genes?.GetGene(FCPGDefOf.FCP_Gene_Ghoul_Glow);
        }
        
        public override Graphic GraphicFor(Pawn pawn)
        {
            if (gene == null) return null;
            Graphic headGraphic = pawn?.Drawer?.renderer?.HeadGraphic;
            
            if (headGraphic == null)
            {
                FCPLog.Warning("[PawnRenderNode_HeadGlow] " +
                               "Could not find base graphic for pawn.");
                return null;
            }
            
            Color glowColor = Color.green;
            if (gene is Gene_Glowing geneChance)
            {
                glowColor = geneChance.GlowColor;
            }
            
            return headGraphic.GetColoredVersion(props.shaderTypeDef.Shader, 
                glowColor, Color.clear);
        }
    }
}