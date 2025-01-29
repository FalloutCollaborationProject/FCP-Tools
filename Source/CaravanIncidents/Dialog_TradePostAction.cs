using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FCP_CaravanIncidents
{
    public class Dialog_TradePostAction : Dialog_Trade
    {
        Action PostCloseAction;
        public Dialog_TradePostAction(Pawn playerNegotiator, ITrader trader, bool giftsOnly = false, Action postCloseAction = null) : base(playerNegotiator, trader, giftsOnly)
        {
            PostCloseAction = postCloseAction;
        }
        public override void PostClose()
        {
            base.PostClose();
            PostCloseAction();
        }
    }
}
