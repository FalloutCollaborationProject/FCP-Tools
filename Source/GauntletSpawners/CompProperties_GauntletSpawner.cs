using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace GauntletSpawners
{
    public class CompProperties_GauntletSpawner : CompProperties
    {
        public List<PawnKindDef> possibleKindDefToSpawn;

        public bool spawnAsDroppod = false;

        public int maxCount = 5;

        public int spawnPerTrigger = 1;

        public int interval = 250;

        public int autoHuntingInterval = 1000;

        public float fuelUsed = 1f;

        public float groupMarkingRadius = 5f;

        public EffecterDef effectOnMark;

        public string uiIcon;

        public string gotoIcon;

        public string attackIcon;

        public bool isOnlyActiveWhenHostileNear;

        public int hostileCheckInterval = 250;

        public float activeRadius = 20f;

        public MentalStateDef mentalStateDef;

        public CompProperties_GauntletSpawner()
        {
            compClass = typeof(CompGauntletSpawner);
        }
    }
}
