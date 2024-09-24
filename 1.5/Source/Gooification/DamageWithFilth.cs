using UnityEngine;
using RimWorld;
using Verse;
using System.Linq;
using System.Collections.Generic;
using Verse.AI.Group;
namespace FalloutCore
{
    public abstract class DamageWithFilth : DamageWorker_AddInjury
    {
        public abstract string FilthToSpawn {  get; }
        public static Dictionary<Thing, DamageInfo> curDinfo = new Dictionary<Thing, DamageInfo>();
        public override DamageResult Apply(DamageInfo dinfo, Thing thing)
        {
            curDinfo[thing] = new DamageInfo(dinfo);
            var result = base.Apply(dinfo, thing);
            curDinfo.Remove(thing);
            return result;
        }
    }

    public class PlasmaBurn : DamageWithFilth
    {
        public override string FilthToSpawn => "FG_Filth_Goop";
    }

    public class LaserBurn : DamageWithFilth
    {
        public override string FilthToSpawn => "FG_Filth_AshPile";
    }
}

