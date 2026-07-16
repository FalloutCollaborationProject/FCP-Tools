using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace FCP.Core.Robotics
{
    public class PawnRenderNodeProperties_RobotApparelOverlay : PawnRenderNodeProperties
    {
        public BodyPartGroupDef targetGroup;
        public float overlayScale = 1f;
        public bool hideOnNorth = false;
    }

    public class RobotApparelEastTexture : DefModExtension
    {
        public string texPath;
    }

    [StaticConstructorOnStartup]
    public static class RobotApparelEastGraphicCache
    {
        private static readonly Dictionary<string, Graphic> Cache = new Dictionary<string, Graphic>();

        static RobotApparelEastGraphicCache()
        {
            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs)
            {
                string texPath = def.GetModExtension<RobotApparelEastTexture>()?.texPath;
                if (texPath != null && !Cache.ContainsKey(texPath))
                {
                    Cache[texPath] = GraphicDatabase.Get<Graphic_Single>(texPath, ShaderDatabase.Cutout, Vector2.one, Color.white);
                }
            }
        }

        public static Graphic GetFor(Apparel apparel)
        {
            string texPath = apparel.def.GetModExtension<RobotApparelEastTexture>()?.texPath;
            return texPath != null && Cache.TryGetValue(texPath, out Graphic graphic) ? graphic : null;
        }
    }

    public class PawnRenderNode_RobotApparelOverlay : PawnRenderNode
    {
        public PawnRenderNode_RobotApparelOverlay(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree)
            : base(pawn, props, tree)
        {
        }

        private PawnRenderNodeProperties_RobotApparelOverlay OverlayProps => (PawnRenderNodeProperties_RobotApparelOverlay)props;

        public float OverlayScale => OverlayProps.overlayScale;
        public bool HideOnNorth => OverlayProps.hideOnNorth;

        public Apparel CurrentApparel(Pawn pawn)
        {
            BodyPartGroupDef group = OverlayProps.targetGroup;
            if (pawn?.apparel == null || group == null)
            {
                return null;
            }

            foreach (Apparel apparel in pawn.apparel.WornApparel)
            {
                if (apparel.def.apparel.bodyPartGroups.Contains(group))
                {
                    return apparel;
                }
            }
            return null;
        }
    }

    public class PawnRenderNodeWorker_RobotApparelOverlay : PawnRenderNodeWorker
    {
        protected override Graphic GetGraphic(PawnRenderNode node, PawnDrawParms parms)
        {
            PawnRenderNode_RobotApparelOverlay casted = (PawnRenderNode_RobotApparelOverlay)node;
            if (casted.HideOnNorth && parms.facing == Rot4.North)
            {
                return null;
            }

            Apparel apparel = casted.CurrentApparel(parms.pawn);
            if (apparel == null)
            {
                return null;
            }

            Graphic graphic = null;
            if (parms.facing == Rot4.East || parms.facing == Rot4.West)
            {
                graphic = RobotApparelEastGraphicCache.GetFor(apparel);
            }
            if (graphic == null)
            {
                graphic = apparel.Graphic;
            }

            CompColorable colorable = apparel.GetComp<CompColorable>();
            if (graphic != null && colorable != null && colorable.Active && colorable.Color != graphic.Color)
            {
                return graphic.GetColoredVersion(graphic.Shader, colorable.Color, graphic.ColorTwo);
            }
            return graphic;
        }

        public override Vector3 ScaleFor(PawnRenderNode node, PawnDrawParms parms)
        {
            float scale = ((PawnRenderNode_RobotApparelOverlay)node).OverlayScale;
            return new Vector3(scale, 1f, scale);
        }
    }
}
