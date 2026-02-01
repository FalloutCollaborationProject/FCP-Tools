using RimWorld;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Verse;
using static RimWorld.QuestGen.QuestNode_GetSitePartDefsByTagsAndFaction;
using static System.Collections.Specialized.BitVector32;

namespace FCP.Core.RadiantQuests
{
    public class QuestNode_GetFactionFromList : QuestNode
    {
        [NoTranslate]
        public SlateRef<string> storeAs;

        public SlateRef<IEnumerable<FactionDef>> factionDefs;

        protected override bool TestRunInt(Slate slate)
        {
            if (Find.FactionManager.GetFactions().Any(c => factionDefs.GetValue(slate).Any(x => x.defName == c.def.defName)))
            {
                Log.Message("factions exist");
                SetVars(slate);
                return true;
            }
            Log.Message("factions dont exist");
            return false;
        }

        protected override void RunInt()
        {
            SetVars(QuestGen.slate);
        }

        private void SetVars(Slate slate)
        {
            Find.FactionManager.GetFactions().Where(c => factionDefs.GetValue(slate).Any(x => x.defName == c.def.defName)).TryRandomElement(out Faction faction);
            Log.Message(faction.def.label);
            slate.Set(storeAs.GetValue(slate), faction);
        }
    }

}

