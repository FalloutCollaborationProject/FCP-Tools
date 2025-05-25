using Verse;

namespace FCP_Ghoul
{
    public class Gene_ToxSpew : Gene
    {
        private ModExtension_Gene_ToxSpew _modExt;
        
        public override void PostAdd()
        {
            base.PostAdd();
            _modExt = def.GetModExtension<ModExtension_Gene_ToxSpew>();
        }
        
        public override void Tick()
        {
            base.Tick();
            if (pawn.Dead || !pawn.Spawned || pawn.Map == null || _modExt == null) 
                return;
            
            if (pawn.IsHashIntervalTick(_modExt.tickInterval))
            {
                GasUtility.AddGas(pawn.Position, pawn.MapHeld, 
                    GasType.ToxGas, _modExt.gasAmount);
            }
        }
    }
}