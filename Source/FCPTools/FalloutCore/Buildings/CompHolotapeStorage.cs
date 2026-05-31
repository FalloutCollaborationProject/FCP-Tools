using System.Collections.Generic;
using RimWorld;
using Verse;

namespace FCP.Core.Buildings
{
    public class CompProperties_HolotapeStorage : CompProperties
    {
        public CompProperties_HolotapeStorage()
        {
            compClass = typeof(CompHolotapeStorage);
        }
    }

    public class CompHolotapeStorage : ThingComp
    {
        private List<Thing> storedHolotapes = new List<Thing>();

        public CompProperties_HolotapeStorage Props => (CompProperties_HolotapeStorage)props;
        public List<Thing> StoredHolotapes => storedHolotapes;
        public int Count => storedHolotapes.Count;

        public bool TryStoreHolotape(Thing holotape)
        {
            if (holotape?.TryGetComp<Holotapes.CompHolotape>() == null)
                return false;

            if (!storedHolotapes.Contains(holotape))
            {
                storedHolotapes.Add(holotape);
                if (holotape.Spawned)
                    holotape.DeSpawn();
                return true;
            }
            return false;
        }

        public Thing TryRemoveHolotape(Thing holotape)
        {
            if (storedHolotapes.Remove(holotape))
            {
                return holotape;
            }
            return null;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref storedHolotapes, "storedHolotapes", LookMode.Deep);
            
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                storedHolotapes ??= new List<Thing>();
                storedHolotapes.RemoveAll(h => h == null);
            }
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            base.PostDestroy(mode, previousMap);
            
            if (mode == DestroyMode.Vanish || storedHolotapes.NullOrEmpty())
                return;

            for (int i = storedHolotapes.Count - 1; i >= 0; i--)
            {
                var holotape = storedHolotapes[i];
                if (holotape != null)
                {
                    GenPlace.TryPlaceThing(holotape, parent.Position, previousMap, ThingPlaceMode.Near);
                }
            }
            storedHolotapes.Clear();
        }

        public override string CompInspectStringExtra()
        {
            if (storedHolotapes.NullOrEmpty())
                return "Stored holotapes: 0";
            
            return $"Stored holotapes: {storedHolotapes.Count}";
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (!storedHolotapes.NullOrEmpty())
            {
                yield return new Command_Action
                {
                    defaultLabel = "Browse holotapes",
                    defaultDesc = "View and read stored holotapes.",
                    icon = ContentFinder<UnityEngine.Texture2D>.Get("Things/Items/Techprints/FCP_Techprint_Holotape_Orange"),
                    action = () =>
                    {
                        Find.WindowStack.Add(new Holotapes.Dialog_HolotapeBrowser(this));
                    }
                };
            }

            if (Prefs.DevMode)
            {
                yield return new Command_Action
                {
                    defaultLabel = "DEV: Clear holotapes",
                    action = () => storedHolotapes.Clear()
                };
            }
        }
    }
}
