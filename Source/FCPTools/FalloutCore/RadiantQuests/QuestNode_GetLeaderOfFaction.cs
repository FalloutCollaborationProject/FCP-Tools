using RimWorld.QuestGen;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FCP.Core.RadiantQuests
{
    public class QuestNode_GetLeaderOfFaction : QuestNode
    {
        [NoTranslate]
        public SlateRef<string> storeAs;

        public SlateRef<FactionDef> factionDef = null;
        public SlateRef<Faction> faction = null;

        public SlateRef<bool> factionMustBePermanent = true;

        protected override bool TestRunInt(Slate slate)
        {
            Log.Message("Testing");
            Log.Message("factionDef is null: ");
            Log.Message(factionDef == null);
            Log.Message("faction is null: ");
            Log.Message(faction == null);
            if (factionDef != null || faction != null)
            {
                SetVars(QuestGen.slate);
                return true;
            }
            return false;
        }

        private bool TryFindFaction(out Faction faction, Slate slate)
        {
            Log.Message(factionDef.GetValue(slate).defName);
            return Find.FactionManager.GetFactions().Where(c => c.def.defName == factionDef.GetValue(slate).defName).TryRandomElement(out faction);
        }

        protected override void RunInt()
        {
            SetVars(QuestGen.slate);
        }

        private void SetVars(Slate slate)
        {
            Log.Message("trying to get faction");
            Faction lfaction = null;
            if (faction != null)
            {
                lfaction = faction.GetValue(slate);
            }
            else
            {
                TryFindFaction(out lfaction, slate);
            }

            Log.Message(lfaction.def.defName);

            Pawn pawn = GetFactionLeader(lfaction);
            Log.Message(pawn.Label); 
/*            QuestPart_InvolvedFactions questPart_InvolvedFactions = new QuestPart_InvolvedFactions();
            Log.Message(1);
            questPart_InvolvedFactions.factions.Add(lfaction);
            Log.Message(2);
            QuestGen.quest.AddPart(questPart_InvolvedFactions);*/
            Log.Message(3);
            QuestGen.slate.Set(storeAs.GetValue(slate), pawn);
            //Log.Message(4);
            //Log.Message(pawn.Label);
        }
        private Pawn GetFactionLeader(Faction faction)
        {
            Log.Message(faction.def.label);
            Log.Message(faction.leader.LabelCap);
            if (faction != null)
            {
                Log.Message("Faction is NOT null");
                return faction.leader;
            }
            return null;
        }

    }
}

