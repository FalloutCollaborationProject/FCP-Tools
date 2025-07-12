using UnityEngine;
using Verse;

namespace FCP_Ghoul
{
    public class PawnRenderNode_CustomColor : PawnRenderNode_Fur
    {
        protected override Shader DefaultShader => ShaderDatabase.CutoutSkinOverlay;
        
        public PawnRenderNode_CustomColor(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree) : base(pawn, props, tree)
        {
        }
        public override Graphic GraphicFor(Pawn pawn)
        {
            return base.GraphicFor(pawn);

        }
        public override Color ColorFor(Pawn pawn)
        {
            if (gene.def==Ghoul_Cache.Fur)
            {
                return pawn.story.SkinColor;
            }

            float r = (pawn.genes.GetGene(Ghoul_Cache.FeralFur) as Gene_FeralBody).r;
            float g = (pawn.genes.GetGene(Ghoul_Cache.FeralFur) as Gene_FeralBody).g;
            float b = (pawn.genes.GetGene(Ghoul_Cache.FeralFur) as Gene_FeralBody).b;
            return new Color(r, g, b);
        }
    }
}
