using Verse;
using FCP.Core;

namespace FCP_Ghoul
{
    [StaticConstructorOnStartup]
    public class Ghoul_Cache
    {
        public static GeneDef ToxHeal;
        public static MentalStateDef PermanentBerserk;
        public static MentalStateDef FeralState;
        public static GeneDef FeralFur;
        public static GeneDef FeralHead;
        public static GeneDef Fur;
        public static GeneDef Head;
        public static GeneDef SkinA;
        public static GeneDef SkinB;
        public static GeneDef SkinC;
        public static GeneDef SkinD;
        public static GeneDef SkinE;
        public static GeneDef SkinFeral;
        public static HediffDef ToxHealHediff;

        static Ghoul_Cache()
        {
            foreach (var geneDef in DefDatabase<GeneDef>.AllDefsListForReading)
            {
                if (ToxHeal is null && geneDef.HasModExtension<ToxHeal_ModExtension>()) ToxHeal = geneDef;
                if (FeralFur is null && geneDef.HasModExtension<FeralFur_ModExtension>()) FeralFur = geneDef;
                if (FeralHead is null && geneDef.HasModExtension<FeralHead_ModExtension>()) FeralHead = geneDef;
                if (Fur is null && geneDef.HasModExtension<Fur_ModExtension>()) Fur = geneDef;
                if (Head is null && geneDef.HasModExtension<Head_ModExtension>()) Head = geneDef;
                if (SkinA is null && geneDef.HasModExtension<SkinA_ModExtension>()) SkinA = geneDef;
                if (SkinB is null && geneDef.HasModExtension<SkinB_ModExtension>()) SkinB = geneDef;
                if (SkinC is null && geneDef.HasModExtension<SkinC_ModExtension>()) SkinC = geneDef;
                if (SkinD is null && geneDef.HasModExtension<SkinD_ModExtension>()) SkinD = geneDef;
                if (SkinE is null && geneDef.HasModExtension<SkinE_ModExtension>()) SkinE = geneDef;
                if (SkinFeral is null && geneDef.HasModExtension<SkinFeral_ModExtension>()) SkinFeral = geneDef;
                
                if (ToxHeal != null && FeralFur != null && FeralHead != null && Fur != null && Head != null &&
                    SkinA != null && SkinB != null && SkinC != null && SkinD != null && SkinE != null && SkinFeral != null)
                    break;
            }
            foreach (var mentalStateDef in DefDatabase<MentalStateDef>.AllDefsListForReading)
            {
                if (mentalStateDef.HasModExtension<PermanentBerserk_ModExtension>())
                {
                    PermanentBerserk = mentalStateDef;
                    break;
                }
            }
            foreach (var hediffDef in DefDatabase<HediffDef>.AllDefsListForReading)
            {
                if (hediffDef.HasModExtension<ToxHealHediff_ModExtension>())
                {
                    ToxHealHediff = hediffDef;
                    break;
                }
            }
            #if DEBUG
            if (ToxHeal == null) FCPLog.Error("Ghoul_Cache: Missing GeneDef with ToxHeal_ModExtension.");
            if (FeralFur == null) FCPLog.Error("Ghoul_Cache: Missing GeneDef with FeralFur_ModExtension.");
            if (FeralHead == null) FCPLog.Error("Ghoul_Cache: Missing GeneDef with FeralHead_ModExtension.");
            if (Fur == null) FCPLog.Error("Ghoul_Cache: Missing GeneDef with Fur_ModExtension.");
            if (Head == null) FCPLog.Error("Ghoul_Cache: Missing GeneDef with Head_ModExtension.");
            if (SkinA == null) FCPLog.Error("Ghoul_Cache: Missing GeneDef with SkinA_ModExtension.");
            if (SkinB == null) FCPLog.Error("Ghoul_Cache: Missing GeneDef with SkinB_ModExtension.");
            if (SkinC == null) FCPLog.Error("Ghoul_Cache: Missing GeneDef with SkinC_ModExtension.");
            if (SkinD == null) FCPLog.Error("Ghoul_Cache: Missing GeneDef with SkinD_ModExtension.");
            if (SkinE == null) FCPLog.Error("Ghoul_Cache: Missing GeneDef with SkinE_ModExtension.");
            if (SkinFeral == null) FCPLog.Error("Ghoul_Cache: Missing GeneDef with SkinFeral_ModExtension.");
            if (PermanentBerserk == null) FCPLog.Error("Ghoul_Cache: Missing MentalStateDef with PermanentBerserk_ModExtension.");
            if (ToxHealHediff == null) FCPLog.Error("Ghoul_Cache: Missing HediffDef with ToxHealHediff_ModExtension.");
            #endif
        }
        
    }
}
