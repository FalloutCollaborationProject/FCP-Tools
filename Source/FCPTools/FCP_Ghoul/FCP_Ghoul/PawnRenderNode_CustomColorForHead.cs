using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;
namespace FCP_Ghoul
{
    class PawnRenderNode_CustomColorForHead : PawnRenderNode_Head
    {
        protected override Shader DefaultShader => ShaderDatabase.CutoutSkinOverlay;
        public PawnRenderNode_CustomColorForHead(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree)
            : base(pawn, props, tree)
        {
        }
        public override Color ColorFor(Pawn pawn)
        {
            if (gene.def == Ghoul_Cache.Head)
            {
                return pawn.story.SkinColor;
            }
            float r = (pawn.genes.GetGene(Ghoul_Cache.FeralHead) as FeralHead_Gene).r;
            float g = (pawn.genes.GetGene(Ghoul_Cache.FeralHead) as FeralHead_Gene).g;
            float b = (pawn.genes.GetGene(Ghoul_Cache.FeralHead) as FeralHead_Gene).b;

            return new Color(r,g,b);
        }
    }
}
