using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace FCP.Core.Robotics
{
    public class PawnRenderNodeProperties_RobotHediffOverlay : PawnRenderNodeProperties
    {
        public BodyPartGroupDef targetGroup;
        public HediffDef requiredHediff;
        public float overlayScale = 1f;
        public Vector2 meshSize = new Vector2(1.4f, 1.4f);
        public bool tintToBodyColor = false;
    }

    public class RobotHediffGraphic : DefModExtension
    {
        public string texPath;
        public string eastTexPath;
        public ShaderTypeDef shaderType;
        public Vector2 drawSize = new Vector2(1.4f, 1.4f);
        public bool isBaseArm = false;
        public bool isBodyOverride = false;
        public Type graphicClass = typeof(Graphic_Multi);
    }

    [StaticConstructorOnStartup]
    public static class RobotHediffGraphicCache
    {
        private static readonly Dictionary<HediffDef, Graphic> Cache = new Dictionary<HediffDef, Graphic>();
        private static readonly Dictionary<string, Graphic> EastCache = new Dictionary<string, Graphic>();
        private static readonly Dictionary<HediffDef, Dictionary<Color, Graphic>> TintedCache = new Dictionary<HediffDef, Dictionary<Color, Graphic>>();

        static RobotHediffGraphicCache()
        {
            foreach (HediffDef def in DefDatabase<HediffDef>.AllDefs)
            {
                RobotHediffGraphic ext = def.GetModExtension<RobotHediffGraphic>();
                if (ext?.texPath == null || Cache.ContainsKey(def))
                {
                    continue;
                }

                Shader shader = ext.shaderType != null ? ext.shaderType.Shader : ShaderDatabase.Cutout;
                Cache[def] = GraphicDatabase.Get(ext.graphicClass, ext.texPath, shader, ext.drawSize, Color.white, Color.white);

                if (ext.eastTexPath != null && !EastCache.ContainsKey(ext.eastTexPath))
                {
                    EastCache[ext.eastTexPath] = GraphicDatabase.Get<Graphic_Single>(ext.eastTexPath, shader, ext.drawSize, Color.white);
                }
            }

            // Pre-build every (overlay graphic x known robot body color) tint combination up front.
            // GetGraphic runs on a parallel draw thread and cannot safely build new colored
            // graphic variants on demand (texture loading must happen on the main thread).
            List<Color> knownBodyColors = CollectKnownBodyColors();
            foreach (KeyValuePair<HediffDef, Graphic> entry in Cache)
            {
                Dictionary<Color, Graphic> tints = new Dictionary<Color, Graphic>();
                foreach (Color color in knownBodyColors)
                {
                    tints[color] = entry.Value.GetColoredVersion(entry.Value.Shader, color, entry.Value.ColorTwo);
                }
                TintedCache[entry.Key] = tints;
            }
        }

        private static List<Color> CollectKnownBodyColors()
        {
            HashSet<Color> colors = new HashSet<Color>();
            foreach (RobotPaintOption paint in RobotPaintOptions.General)
            {
                colors.Add(paint.color);
            }
            foreach (RobotPaintOption paint in RobotPaintOptions.Factional)
            {
                colors.Add(paint.color);
            }

            foreach (PawnKindDef kindDef in DefDatabase<PawnKindDef>.AllDefs)
            {
                if (kindDef.race?.comps == null || !kindDef.race.comps.Any(c => c is CompProperties_RobotManualPower))
                {
                    continue;
                }

                GraphicData bodyGraphicData = kindDef.lifeStages?.FirstOrDefault()?.bodyGraphicData;
                if (bodyGraphicData != null)
                {
                    colors.Add(bodyGraphicData.color);
                }
            }

            return colors.ToList();
        }

        public static Graphic GetFor(HediffDef def)
        {
            return Cache.TryGetValue(def, out Graphic graphic) ? graphic : null;
        }

        public static Graphic GetEastFor(HediffDef def)
        {
            string texPath = def.GetModExtension<RobotHediffGraphic>()?.eastTexPath;
            return texPath != null && EastCache.TryGetValue(texPath, out Graphic graphic) ? graphic : null;
        }

        public static Graphic GetTintedFor(HediffDef def, Color color)
        {
            if (TintedCache.TryGetValue(def, out Dictionary<Color, Graphic> tints) && tints.TryGetValue(color, out Graphic graphic))
            {
                return graphic;
            }
            // Unknown body color (e.g. from a modded source) - fall back to the untinted
            // graphic rather than risk an off-thread texture load.
            return GetFor(def);
        }
    }

    public class PawnRenderNode_RobotHediffOverlay : PawnRenderNode
    {
        public PawnRenderNode_RobotHediffOverlay(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree)
            : base(pawn, props, tree)
        {
        }

        private PawnRenderNodeProperties_RobotHediffOverlay HediffProps => (PawnRenderNodeProperties_RobotHediffOverlay)props;

        public float OverlayScale => HediffProps.overlayScale;
        public bool TintToBodyColor => HediffProps.tintToBodyColor;

        public override GraphicMeshSet MeshSetFor(Pawn pawn)
        {
            return MeshPool.GetMeshSetForSize(HediffProps.meshSize.x, HediffProps.meshSize.y);
        }

        public Hediff CurrentHediff(Pawn pawn)
        {
            if (pawn?.health?.hediffSet == null)
            {
                return null;
            }

            HediffSet hediffSet = pawn.health.hediffSet;

            if (HediffProps.requiredHediff != null)
            {
                return hediffSet.hediffs.FirstOrDefault(h =>
                    h.def == HediffProps.requiredHediff && (h.Part == null || !hediffSet.PartIsMissing(h.Part)));
            }

            BodyPartGroupDef group = HediffProps.targetGroup;
            if (group == null)
            {
                return null;
            }

            return hediffSet.hediffs.FirstOrDefault(h =>
                h.Part != null && h.Part.groups.Contains(group) && !hediffSet.PartIsMissing(h.Part)
                && h.def.GetModExtension<RobotHediffGraphic>() is RobotHediffGraphic rhg && !rhg.isBaseArm);
        }
    }

    public class PawnRenderNodeWorker_RobotHediffOverlay : PawnRenderNodeWorker
    {
        protected override Graphic GetGraphic(PawnRenderNode node, PawnDrawParms parms)
        {
            PawnRenderNode_RobotHediffOverlay casted = (PawnRenderNode_RobotHediffOverlay)node;
            Hediff hediff = casted.CurrentHediff(parms.pawn);
            if (hediff == null)
            {
                return null;
            }

            Graphic graphic = null;
            if (parms.facing == Rot4.East || parms.facing == Rot4.West)
            {
                graphic = RobotHediffGraphicCache.GetEastFor(hediff.def);
            }
            if (graphic == null)
            {
                graphic = RobotHediffGraphicCache.GetFor(hediff.def);
            }
            if (graphic == null || !casted.TintToBodyColor)
            {
                return graphic;
            }

            Color bodyColor = RobotUtility.GetBodyColor(parms.pawn);
            if (bodyColor != graphic.Color)
            {
                return RobotHediffGraphicCache.GetTintedFor(hediff.def, bodyColor);
            }
            return graphic;
        }

        public override Vector3 ScaleFor(PawnRenderNode node, PawnDrawParms parms)
        {
            float scale = ((PawnRenderNode_RobotHediffOverlay)node).OverlayScale;
            return new Vector3(scale, 1f, scale);
        }

        public override MaterialPropertyBlock GetMaterialPropertyBlock(PawnRenderNode node, Material material, PawnDrawParms parms)
        {
            return base.GetMaterialPropertyBlock(node, material, parms) ?? node.MatPropBlock;
        }
    }
}
