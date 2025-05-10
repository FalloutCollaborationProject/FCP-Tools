using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
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
            ToxHeal = DefDatabase<GeneDef>.AllDefsListForReading.Where(x => x.HasModExtension<ToxHeal_ModExtension>() == true).First();
            PermanentBerserk = DefDatabase<MentalStateDef>.AllDefsListForReading.
                Where(x => x.HasModExtension<PermanentBerserk_ModExtension>() == true).First();
            FeralFur = DefDatabase<GeneDef>.AllDefsListForReading.Where(x=>x.HasModExtension<FeralFur_ModExtension>()==true).First();
            FeralHead=DefDatabase<GeneDef>.AllDefsListForReading.Where(x => x.HasModExtension<FeralHead_ModExtension>() == true).First();
            Fur = DefDatabase<GeneDef>.AllDefsListForReading.Where(x => x.HasModExtension<Fur_ModExtension>() == true).First();
            Head= DefDatabase<GeneDef>.AllDefsListForReading.Where(x => x.HasModExtension<Head_ModExtension>() == true).First();
            SkinA= DefDatabase<GeneDef>.AllDefsListForReading.Where(x => x.HasModExtension<SkinA_ModExtension>() == true).First();
            SkinB = DefDatabase<GeneDef>.AllDefsListForReading.Where(x => x.HasModExtension<SkinB_ModExtension>() == true).First();
            SkinC = DefDatabase<GeneDef>.AllDefsListForReading.Where(x => x.HasModExtension<SkinC_ModExtension>() == true).First();
            SkinD = DefDatabase<GeneDef>.AllDefsListForReading.Where(x => x.HasModExtension<SkinD_ModExtension>() == true).First();
            SkinE = DefDatabase<GeneDef>.AllDefsListForReading.Where(x => x.HasModExtension<SkinE_ModExtension>() == true).First();
            SkinFeral= DefDatabase<GeneDef>.AllDefsListForReading.Where(x => x.HasModExtension<SkinFeral_ModExtension>() == true).First();
            ToxHealHediff= DefDatabase<HediffDef>.AllDefsListForReading.Where(x => x.HasModExtension<ToxHealHediff_ModExtension>() == true).First();
        }
        
    }
}
