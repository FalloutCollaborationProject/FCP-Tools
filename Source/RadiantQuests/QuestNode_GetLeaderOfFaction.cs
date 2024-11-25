using RimWorld.QuestGen;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FCP_RadiantQuests
{
    public class QuestNode_GetLeaderOfFaction : QuestNode
    {
        [NoTranslate]
        public SlateRef<string> storeAs;

        public SlateRef<FactionDef> factionDef;

        public SlateRef<bool> factionMustBePermanent = true;

        protected override bool TestRunInt(Slate slate)
        {
            Log.Message("Testing");
            if (!TryFindFaction(out var faction))
            {
                Log.Message("Faction not found");
                return false;
            }
            if (!TryGetFactionLeader(faction, out var leader))
            {
                Log.Message("Leader not found");
                return false;
            }
            Log.Message("Faction and leader found");
            return true;
        }

        private bool TryFindFaction(out Faction faction)
        {
            return Find.FactionManager.GetFactions().Where(c => c.def == factionDef).TryRandomElement(out faction);
        }

        protected override void RunInt()
        {
            Slate slate = QuestGen.slate;
            TryFindFaction(out Faction faction);
            TryGetFactionLeader(faction, out Pawn pawn);
            //Log.Message(pawn.Label); 
            QuestPart_InvolvedFactions questPart_InvolvedFactions = new QuestPart_InvolvedFactions();
            questPart_InvolvedFactions.factions.Add(faction);
            QuestGen.quest.AddPart(questPart_InvolvedFactions);
            QuestGen.slate.Set(storeAs.GetValue(slate), pawn);

        }
        private bool TryGetFactionLeader(Faction faction, out Pawn pawn)
        {
            //Log.Message(faction.def.label);
            //Log.Message(faction.leader.LabelCap);
            if (faction != null)
            {
                pawn = faction.leader;
                return true;
            }
            pawn = null;
            return false;
        }

    }
}
