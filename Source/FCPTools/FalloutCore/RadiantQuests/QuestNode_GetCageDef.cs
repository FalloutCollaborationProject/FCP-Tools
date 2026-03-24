using RimWorld.QuestGen;

namespace FCP.Core.RadiantQuests
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

