using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
namespace FCP_Ghoul
{
    public class EmitToxGene : Gene
    {
        public override void Tick()
        {
            base.Tick();
            if (pawn.IsHashIntervalTick(300))
            {
                if (pawn.Spawned)
                {
                    GasUtility.AddGas(pawn.Position, pawn.MapHeld, GasType.ToxGas, 50);
                }
            }
            
            
        }
    }
}
