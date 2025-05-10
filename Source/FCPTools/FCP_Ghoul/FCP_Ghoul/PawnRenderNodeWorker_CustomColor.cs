using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
namespace FCP_Ghoul
{
    public class PawnRenderNodeWorker_CustomColor : PawnRenderNodeWorker_Fur
    {
        public override bool CanDrawNow(PawnRenderNode node, PawnDrawParms parms)
        {
            return base.CanDrawNow(node, parms);
        }
    }
}
