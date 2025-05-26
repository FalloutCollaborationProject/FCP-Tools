using Verse;

namespace FCP_Ghoul
{
    public class ChanceGene : Gene
    {
        public override void PostAdd()
        {
            base.PostAdd();
            if (Rand.Chance(this.def.GetModExtension<ChanceGene_ModExtension>().chance))
            {
                pawn.genes.AddGene(def.GetModExtension<ChanceGene_ModExtension>().gene1,false);
                pawn.genes.AddGene(def.GetModExtension<ChanceGene_ModExtension>().gene2, false);
            }
        }
    }
}
