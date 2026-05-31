using System.Collections.Generic;
using RimWorld;
using Verse;

namespace FCP.Core.Holotapes
{
    public class CompPipboyHolotapeStorage : ThingComp
    {
        private List<Thing> storedHolotapes = new List<Thing>();

        public List<Thing> StoredHolotapes => storedHolotapes;
        public int Count => storedHolotapes.Count;

        public bool TryStoreHolotape(Thing holotape)
        {
            if (holotape == null) return false;
            
            if (holotape.Spawned)
                holotape.DeSpawn();
            
            storedHolotapes.Add(holotape);
            return true;
        }

        public Thing TryRemoveHolotape(Thing holotape)
        {
            if (storedHolotapes.Remove(holotape))
                return holotape;
            return null;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref storedHolotapes, "storedHolotapes", LookMode.Deep);
            
            if (Scribe.mode == LoadSaveMode.PostLoadInit && storedHolotapes == null)
                storedHolotapes = new List<Thing>();
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            base.PostDestroy(mode, previousMap);
            if (mode != DestroyMode.KillFinalize || previousMap == null)
                return;
            
            for (int i = storedHolotapes.Count - 1; i >= 0; i--)
            {
                Thing holotape = storedHolotapes[i];
                if (holotape == null)
                    continue;
                
                IntVec3 pos = parent.Position;
                if (!pos.IsValid && parent is Apparel apparel && apparel.Wearer != null)
                    pos = apparel.Wearer.Position;
                
                if (pos.IsValid)
                    GenPlace.TryPlaceThing(holotape, pos, previousMap, ThingPlaceMode.Near);
            }
        }

        public override string CompInspectStringExtra()
        {
            if (storedHolotapes.Count > 0)
                return "Stored holotapes: " + storedHolotapes.Count;
            return null;
        }
    }

    public class CompProperties_PipboyHolotapeStorage : CompProperties
    {
        public CompProperties_PipboyHolotapeStorage()
        {
            compClass = typeof(CompPipboyHolotapeStorage);
        }
    }
}
