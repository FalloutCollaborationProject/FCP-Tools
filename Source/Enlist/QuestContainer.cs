using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace  FCP.Enlist
{
    public class QuestContainer : IExposable
    {
        public Dictionary<WorldObject, QuestsContainer> availableQuests;

        public QuestContainer()
        {
            availableQuests = new Dictionary<WorldObject, QuestsContainer>();
        }
        public void ExposeData()
        {
            Scribe_Collections.Look(ref availableQuests, "availableQuests", LookMode.Reference, LookMode.Deep, ref worldObjectKeys, ref questValues);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                availableQuests ??= new Dictionary<WorldObject, QuestsContainer>();
            }
        }

        private List<WorldObject> worldObjectKeys;
        private List<QuestsContainer> questValues;
    }
}
