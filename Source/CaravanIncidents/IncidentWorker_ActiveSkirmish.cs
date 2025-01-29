using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FCP_CaravanIncidents
{
    public class IncidentWorker_ActiveSkirmish : IncidentWorker_MultiPartBase
    {

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if (!base.CanFireNowSub(parms))
            {
                return false;
            }
            if (!CaravanIncidents_Settings.enableActiveSkirmish)
            {
                return false;
            }
            return true;
        }
        public override void ActionApproach(Caravan caravan, IncidentParms parms)
        {
            int num = Rand.Range(1, CaravanIncidents_Settings.activeSkirmishWeightsTotal);
            Log.Message(num);

            if (num < CaravanIncidents_Settings.cumulativeWeightsActiveSkirmish[0])
            {
                Log.Message(1);
                DiaNode diaNode = new DiaNode("FCPActiveSkirmishVariantA".Translate());
                DiaOption diaOption = new DiaOption("OK".Translate());
                diaOption.action = delegate
                {
                    QuestScriptDef def = DefDatabase<QuestScriptDef>.AllDefs.Where(c => c.defName.Contains("FCP_Quest_ActiveSkirmish_A")).RandomElement();
                    Quest quest = IncidentUtility.GenerateCaravanQuest(def, parms.points, (Caravan)parms.target);


                };
                diaOption.resolveTree = true;
                diaNode.options.Add(diaOption);
                Find.WindowStack.Add(new Dialog_NodeTree(diaNode, true, false));
            }
            else
            if (num < CaravanIncidents_Settings.cumulativeWeightsActiveSkirmish[1])
            {
                Log.Message(2);
                DiaNode diaNode = new DiaNode("FCPShuttleCrashVariantB".Translate());
                DiaOption diaOption = new DiaOption("OK".Translate());
                diaOption.action = delegate
                {
                    QuestScriptDef def = DefDatabase<QuestScriptDef>.AllDefs.Where(c => c.defName.Contains("FCP_Quest_ActiveSkirmish_B")).RandomElement();
                    Quest quest = IncidentUtility.GenerateCaravanQuest(def, parms.points, (Caravan)parms.target);


                };
                diaOption.resolveTree = true;
                diaNode.options.Add(diaOption);
                Find.WindowStack.Add(new Dialog_NodeTree(diaNode, true, false));
            }
            else
            if (num < CaravanIncidents_Settings.cumulativeWeightsActiveSkirmish[2])
            {
                Log.Message(3);
                DiaNode diaNode = new DiaNode("FCPShuttleCrashVariantC".Translate());
                DiaOption diaOption = new DiaOption("OK".Translate());
                diaOption.action = delegate
                {
                    QuestScriptDef def = DefDatabase<QuestScriptDef>.AllDefs.Where(c => c.defName.Contains("FCP_Quest_ActiveSkirmish_C")).RandomElement();
                    Quest quest = IncidentUtility.GenerateCaravanQuest(def, parms.points, (Caravan)parms.target);


                };
                diaOption.resolveTree = true;
                diaNode.options.Add(diaOption);
                Find.WindowStack.Add(new Dialog_NodeTree(diaNode, true, false));
            }
        }

        public override void ActionIgnore(Caravan caravan, IncidentParms parms)
        {
            return;
        }

        public override TaggedString GetText()
        {
            return "FCPShuttleCrashIntroText".Translate();
        }

        public override TaggedString GetTitle()
        {
            return "FCPShuttleCrashIntroTitle".Translate();
        }
    }
}
