using JetBrains.Annotations;
using UnityEngine;
using Verse;

namespace FCP_Ghoul
{
    [UsedImplicitly]
    public class Gene_GhoulSkin : Gene
    {
        public Color SkinColor;
        public string ColorLabel;

        private ModExtension_Gene_GhoulSkin _modExtSkin;
        private readonly string _labelBase = "Ghoul skin";
        
        public override string Label => _labelBase + $" ({ColorLabel})";
        
        public override void PostMake()
        {
            base.PostMake();
            _modExtSkin = def.GetModExtension<ModExtension_Gene_GhoulSkin>();
            
            if (_modExtSkin != null)
            {
                SkinColor = new Color(
                    _modExtSkin.redRange.RandomInRange,
                    _modExtSkin.greenRange.RandomInRange,
                    _modExtSkin.blueRange.RandomInRange);
            }
            else
            {
                SkinColor = Color.white;
            }
            
            ColorLabel = GeneColorUtil.ClosestColorName(SkinColor);
        }
        
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref SkinColor, "SkinColor", Color.white);
            Scribe_Values.Look(ref ColorLabel, "ColorLabel", "unknown");
        }
    }
}