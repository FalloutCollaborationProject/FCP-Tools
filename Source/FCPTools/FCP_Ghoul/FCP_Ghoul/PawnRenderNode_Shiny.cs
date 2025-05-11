using UnityEngine;
using Verse;

namespace FCP_Ghoul
{
    public class PawnRenderNode_Shiny : PawnRenderNode
    {
        public override Shader DefaultShader => ShaderDatabase.MoteGlow;
        public PawnRenderNode_Shiny(Pawn pawn,PawnRenderNodeProperties props, PawnRenderTree tree) : base(pawn, props, tree)
        {

        }
        public override GraphicMeshSet MeshSetFor(Pawn pawn)
        {
            Graphic graphic = GraphicFor(pawn);
            if (graphic != null)
            {
                return MeshPool.GetMeshSetForSize(graphic.drawSize.x, graphic.drawSize.y);
            }
            return null;
        }
        public override Graphic GraphicFor(Pawn pawn)
        {
            string text = TexPathFor(pawn);
            if (text.NullOrEmpty())
            {
                return null;
            }

            return GraphicDatabase.Get<Graphic_Multi>(text, ShaderDatabase.MoteGlow, Vector2.one, new Color(0.8f, 0.9f, 0.8f, 1f));
        }
    }
}
