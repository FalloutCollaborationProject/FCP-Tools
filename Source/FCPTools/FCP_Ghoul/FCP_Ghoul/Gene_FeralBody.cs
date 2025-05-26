using Verse;

namespace FCP_Ghoul
{
    public class Gene_FeralBody : Gene
    {
        public float r;
        public float g;
        public float b;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref r, "r");
            Scribe_Values.Look(ref g, "g");
            Scribe_Values.Look(ref b, "b");
        }
        public override void Tick()
        {
            base.Tick();
            if (!pawn.Dead && pawn.Spawned && pawn.Map != null)
            {
                if (pawn.IsHashIntervalTick(this.def.GetModExtension<ToxBomb_ModExtension>().rate))
                {
                    GasUtility.AddGas(pawn.Position, pawn.Map, GasType.ToxGas, def.GetModExtension<ToxBomb_ModExtension>().radius);
                }
            }

        }
    }
}
