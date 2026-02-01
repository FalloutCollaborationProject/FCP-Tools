using UnityEngine;
using Verse;

namespace FCP.Core.Ghouls
{
    public class Gene_GhoulBody : Gene
    {
        private Graphic cachedGraphic;
        private string cachedBodyType;
        private Color cachedSkinColor;

        public override void PostAdd()
        {
            base.PostAdd();
            pawn.Drawer.renderer.SetAllGraphicsDirty();
        }

        public override void PostRemove()
        {
            base.PostRemove();
            pawn.Drawer.renderer.SetAllGraphicsDirty();
        }

        public Graphic GetBodyOverlay(Pawn pawn)
        {
            string bodyType = pawn.story.bodyType.defName;
            Color skinColor = pawn.story.SkinColor;
            
            if (cachedGraphic != null && cachedBodyType == bodyType && cachedSkinColor == skinColor)
                return cachedGraphic;
            
            bool isFeral = def.defName == "FCP_Gene_Ghoul_Feral_Skin";
            string path = isFeral ? $"FCP_Ghoul/Feral/Bodies/Naked_{bodyType}" : $"FCP_Ghoul/Bodies/Naked_{bodyType}";
            
            cachedGraphic = GraphicDatabase.Get<Graphic_Multi>(path, ShaderDatabase.CutoutSkin, Vector2.one * 1.5f, skinColor);
            cachedBodyType = bodyType;
            cachedSkinColor = skinColor;
            
            return cachedGraphic;
        }
    }
}
