using Verse;
using RimWorld;
using System.Collections.Generic;

namespace FCP.Core
{
    public class MercenaryCaravanData : IExposable
    {
        public Faction faction;
        public List<Pawn> pawns = new List<Pawn>();
        public CaravanObjectiveDef objective;
        public int departureTick;
        public int returnTick;
        public float successChance;
        public MercenaryCaravanData() { }

        public MercenaryCaravanData(Faction faction, List<Pawn> pawns, CaravanObjectiveDef objective, int departureTick, int returnTick, float successChance)
        {
            this.faction = faction;
            this.pawns.AddRange(pawns);
            this.objective = objective;
            this.departureTick = departureTick;
            this.returnTick = returnTick;
            this.successChance = successChance;
        }

        public void ExposeData()
        {
            Scribe_References.Look(ref faction, "faction");
            Scribe_Collections.Look(ref pawns, "pawns", LookMode.Reference);
            Scribe_Defs.Look(ref objective, "objective");
            Scribe_Values.Look(ref departureTick, "departureTick");
            Scribe_Values.Look(ref returnTick, "returnTick");
            Scribe_Values.Look(ref successChance, "successChance");

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                pawns ??= new List<Pawn>();
                pawns.RemoveAll(p => p == null);
            }
        }
    }
}