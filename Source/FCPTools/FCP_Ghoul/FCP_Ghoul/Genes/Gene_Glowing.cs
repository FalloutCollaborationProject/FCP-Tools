using JetBrains.Annotations;
using UnityEngine;
using Verse;

namespace FCP_Ghoul
{
    [UsedImplicitly]
    public class Gene_Glowing : Gene
    {
        public Color GlowColor;
        
        private ModExtension_Gene_Glowing _modExtGlow;
        
        public override void PostMake()
        {
            base.PostMake();
            _modExtGlow = def.GetModExtension<ModExtension_Gene_Glowing>();
            
            if (_modExtGlow != null)
            {
                GlowColor = new Color(
                    _modExtGlow.redRange.RandomInRange,
                    _modExtGlow.greenRange.RandomInRange,
                    _modExtGlow.blueRange.RandomInRange);
            }
            else
            {
                float t = Rand.Range(0f, 1f);
                GlowColor = new Color(Mathf.Lerp(1f, 0f, t), 1f, 0f);
            }
        }
        
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref GlowColor, "GlowColor", Color.white);
        }
    }
}