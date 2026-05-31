using System.Collections.Generic;
using Verse;

namespace FCP.Core.Holotapes
{
    public class HolotapeTracker : GameComponent
    {
        private HashSet<string> discoveredHolotapes = new HashSet<string>();
        private static HolotapeTracker instance;

        public static HolotapeTracker Instance
        {
            get
            {
                if (instance == null)
                    instance = Current.Game.GetComponent<HolotapeTracker>();
                return instance;
            }
        }

        public HolotapeTracker(Game game)
        {
            instance = this;
        }

        public bool IsDiscovered(HolotapeDef def)
        {
            return discoveredHolotapes.Contains(def.defName);
        }

        public void MarkAsDiscovered(HolotapeDef def)
        {
            if (def != null && !discoveredHolotapes.Contains(def.defName))
            {
                discoveredHolotapes.Add(def.defName);
            }
        }

        public List<HolotapeDef> GetUndiscoveredHolotapes()
        {
            var undiscovered = new List<HolotapeDef>();
            foreach (var def in DefDatabase<HolotapeDef>.AllDefsListForReading)
            {
                if (!IsDiscovered(def))
                    undiscovered.Add(def);
            }
            return undiscovered;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref discoveredHolotapes, "discoveredHolotapes", LookMode.Value);
            
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                discoveredHolotapes ??= new HashSet<string>();
            }
        }
    }
}
