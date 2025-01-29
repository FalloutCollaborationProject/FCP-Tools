using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace FCP_CaravanIncidents
{
    public abstract class IncidentWorker_MultiPartBase : IncidentWorker
    {

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            Log.Message(CaravanIncidentUtility.CanFireIncidentWhichWantsToGenerateMapAt(parms.target.Tile));
            if (CaravanIncidentUtility.CanFireIncidentWhichWantsToGenerateMapAt(parms.target.Tile))
            {
                return true;
            }
            return false;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Caravan caravan = (Caravan)parms.target;
            CameraJumper.TryJumpAndSelect(caravan);
            DiaNode diaNode = new DiaNode(GetText());
            DiaOption diaOption = new DiaOption("Approach".Translate());
            diaOption.action = delegate
            {
                Log.Message("Approaching");
                ActionApproach(caravan, parms);
            };
            diaOption.resolveTree = true;
            diaNode.options.Add(diaOption);
            DiaOption diaOption2 = new DiaOption("CaravanMeeting_MoveOn".Translate());
            diaOption2.action = delegate
            {
                ActionIgnore(caravan, parms);
            };
            diaOption2.resolveTree = true;
            diaNode.options.Add(diaOption2);
            Find.WindowStack.Add(new Dialog_NodeTree(diaNode, true, false, GetTitle()));
            Find.Archive.Add(new ArchivedDialog(diaNode.text, GetTitle()));
            return true;
        }

        public abstract TaggedString GetTitle();
        public abstract TaggedString GetText();
        public abstract void ActionApproach(Caravan caravan, IncidentParms parms);

        public abstract void ActionIgnore(Caravan caravan, IncidentParms parms);
    }
}
