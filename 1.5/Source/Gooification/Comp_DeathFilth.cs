using UnityEngine;
using RimWorld;
using Verse;
using System.Linq;
using System.Text;
using Verse.AI;

namespace FalloutCore
{
    public class CompDeathFilth : ThingComp
    {
        public CompProperties_DeathFilth Props => this.props as CompProperties_DeathFilth;
        public string thingToSpawn = null;
        public bool Dead = false;
        public float chance = 0;
        public void TryToMakeDamageFilth()
        {
            if (this.Dead != true)
            {
                if (this.parent is Pawn pawn && pawn.Corpse != null && !pawn.Corpse.Destroyed)
                {
                    float roll = Rand.Range(0f, 100f);
                    if (roll <= chance)
                    {
                        Thing thing = ThingMaker.MakeThing(ThingDef.Named(thingToSpawn));
                        GenSpawn.Spawn(thing, pawn.Corpse.Position, pawn.Corpse.Map, WipeMode.Vanish);
                        pawn.Corpse.Destroy(DestroyMode.Vanish);
                        this.parent.AllComps.Remove(this);
                        this.Dead = true;
                    }
                }
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<string>(ref this.thingToSpawn, "thingToSpawn", null, false);
            Scribe_Values.Look<bool>(ref this.Dead, "Dead", false, false);
            Scribe_Values.Look<float>(ref this.chance, "chance", 0f, false);
        }
    }
}

