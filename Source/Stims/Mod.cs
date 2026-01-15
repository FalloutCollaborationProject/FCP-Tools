using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace StimPacks
{
    [StaticConstructorOnStartup]
    // ReSharper disable once ClassNeverInstantiated.Global
    public class StimPacks
    {
        public static Dictionary<BodyDef, int> TotalHpForRace = new Dictionary<BodyDef, int>();
        public static int AverageHitPoint => 1000;
        static StimPacks()
        {
            foreach (BodyDef def in DefDatabase<BodyDef>.AllDefs)
            {
                int hp = 0;
                foreach (var part in def.AllParts)
                {
                    hp += part.def.hitPoints;
                }
                TotalHpForRace.Add(def, hp);
            }
            Harmony harmony = new  Harmony("Eragon.Rick.StimPack");
            harmony.PatchAll();
        }
    }
}
