using RimWorld;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FCP.Core.RadiantQuests
{
    public class QuestNode_GetPawnKind : QuestNode
    {
        [NoTranslate]
        public SlateRef<string> storeAs;

        public SlateRef<string> pawnKindDef;

        protected override bool TestRunInt(Slate slate)
        {
            if (DefDatabase<PawnKindDef>.AllDefsListForReading.Any(c => c.defName == pawnKindDef.GetValue(slate)))
            {
                Log.Message("pawnkind exist");
                SetVars(slate);
                return true;
            }
            Log.Message("pawnkind dont exist");
            return false;
        }

        protected override void RunInt()
        {
            SetVars(QuestGen.slate);
        }

        private void SetVars(Slate slate)
        {
            PawnKindDef def = DefDatabase<PawnKindDef>.AllDefsListForReading.Where(c => c.defName == pawnKindDef.GetValue(slate)).First();
            Log.Message(def.defName);
            slate.Set(storeAs.GetValue(slate), def);
        }
    }
}

