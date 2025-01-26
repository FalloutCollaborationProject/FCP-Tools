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
    public class IncidentWorker_ShuttleCrash : IncidentWorker_MultiPartBase
    {
        public override void ActionApproach(Caravan caravan, IncidentParms parms)
        {
            int num = Rand.Range(1, CaravanIncidents_Settings.shuttleWeightsTotal);
            Log.Message(num);
          
            if (num < CaravanIncidents_Settings.cumulativeWeightsShuttleCrash[0])
            {
                Log.Message(1);
                DiaNode diaNode = new DiaNode("FCPShuttleCrashVariantA".Translate());
                DiaOption diaOption = new DiaOption("OK".Translate());
                diaOption.action = delegate
                {
                    QuestScriptDef def = DefDatabase<QuestScriptDef>.AllDefs.Where(c => c.defName.Contains("FCP_Quest_CaravanIncident_A")).RandomElement();
                    Quest quest = IncidentUtility.GenerateCaravanQuest(def, parms.points, (Caravan)parms.target);
                    
                    
                };
                diaOption.resolveTree = true;
                diaNode.options.Add(diaOption);
                Find.WindowStack.Add(new Dialog_NodeTree(diaNode, true, false));
            }
            else
            if (num < CaravanIncidents_Settings.cumulativeWeightsShuttleCrash[1])
            {
                Log.Message(2);
                DiaNode diaNode = new DiaNode("FCPShuttleCrashVariantB".Translate());
                DiaOption diaOption = new DiaOption("OK".Translate());
                diaOption.action = delegate
                {
                    QuestScriptDef def = DefDatabase<QuestScriptDef>.AllDefs.Where(c => c.defName.Contains("FCP_Quest_CaravanIncident_B")).RandomElement();
                    Quest quest = IncidentUtility.GenerateCaravanQuest(def, parms.points, (Caravan)parms.target);


                };
                diaOption.resolveTree = true;
                diaNode.options.Add(diaOption);
                Find.WindowStack.Add(new Dialog_NodeTree(diaNode, true, false));
            }
            else
            if (num < CaravanIncidents_Settings.cumulativeWeightsShuttleCrash[2])
            {
                Log.Message(3);
                DiaNode diaNode = new DiaNode("FCPShuttleCrashVariantC".Translate());
                DiaOption diaOption = new DiaOption("OK".Translate());
                diaOption.action = delegate
                {
                    QuestScriptDef def = DefDatabase<QuestScriptDef>.AllDefs.Where(c => c.defName.Contains("FCP_Quest_CaravanIncident_C")).RandomElement();
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
