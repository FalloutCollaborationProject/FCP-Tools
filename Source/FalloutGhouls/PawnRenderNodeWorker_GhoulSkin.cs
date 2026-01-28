using UnityEngine;
using Verse;

namespace FCP_Ghoul
{
    public class PawnRenderNodeWorker_GhoulSkin : PawnRenderNodeWorker_Fur
    {
        public override Color ColorFor(PawnRenderNode node)
        {
            // Force the body to use the exact same color as the pawn's skin
            return node.Props.color.GetValueOrDefault(node.tree.pawn.story.SkinColor);
        }
    }
}
