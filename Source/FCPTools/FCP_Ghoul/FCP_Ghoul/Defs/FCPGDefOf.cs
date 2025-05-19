using RimWorld;
using Verse;

namespace FCP_Ghoul
{
    [DefOf]
    public static class FCPGDefOf
    {
        public static GeneDef FCP_Gene_ToxHeal;
        public static GeneDef FCP_Skin_Ghoul_Feral;
        public static GeneDef FCP_Skin_Ghoul_A;
        public static GeneDef FCP_Skin_Ghoul_B;
        public static GeneDef FCP_Skin_Ghoul_C;
        public static GeneDef FCP_Skin_Ghoul_D;
        public static GeneDef FCP_Skin_Ghoul_E;
        public static GeneDef FCP_Gene_Ghoul_Glow;
        public static GeneDef FCP_Gene_Ghoul_ToxSpew;
        public static GeneDef FCP_Gene_Ghoul_Manic;
        public static GeneDef FCP_Ghoul_SkinColor;
        public static GeneDef FCP_Ghoul_Skin_Head;
        public static GeneDef FCP_Ghoul_Skin_Body;
        
        public static XenotypeDef FCP_Xenotype_Ghoul;
        public static XenotypeDef FCP_Xenotype_Ghoul_Glowing_One;
        
        public static HediffDef FCP_Hediff_ToxHeal;
        
        public static MentalStateDef FCP_MentalState_PermanentBerserk;
        
        static FCPGDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(FCPGDefOf));
        }
    }
}