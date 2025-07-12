using RimWorld;
using System.Collections.Generic;
using Verse;

namespace  FCP.Enlist
{
    public class FactionState : IExposable
    {
        public List<FactionRelation> factionRelationships = new List<FactionRelation>();

        public void ExposeData()
        {
            Scribe_Collections.Look(ref factionRelationships, "factionRelationships", LookMode.Deep);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                factionRelationships ??= new List<FactionRelation>();
            }
        }
    } 
}
