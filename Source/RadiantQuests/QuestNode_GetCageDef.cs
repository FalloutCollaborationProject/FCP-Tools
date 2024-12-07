using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI.Group;
using Verse.Noise;

namespace FCP_RadiantQuests
{
    public class QuestNode_GetCageDef : QuestNode
    {
        [NoTranslate]
        public SlateRef<string> storeAs;
        public SlateRef<string> cageDef;

        protected override bool TestRunInt(Slate slate)
        {
            SetVars(slate);
            return true;
        }

        protected override void RunInt()
        {
            SetVars(QuestGen.slate);
        }

        private void SetVars(Slate slate)
        {
            if (cageDef != null)
            {
                ThingDef def = DefDatabase<ThingDef>.AllDefsListForReading.Where(c => c.defName == cageDef.GetValue(slate)).First();
                slate.Set(storeAs.GetValue(slate), def);

            }

        }
    }
}
