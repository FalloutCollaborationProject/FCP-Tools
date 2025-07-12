using UnityEngine;
using RimWorld;
using Verse;
using System.Linq;
using System.Text;
using Verse.AI;

namespace FalloutCore
{

    public class Comp_GlowingGhoul : CompGlower
    {

        public new CompProperties_GlowingGhoul Props => this.props as CompProperties_GlowingGhoul;

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
        }

        public override void PostPostMake()
        {
            base.PostPostMake();
        }

        public IntVec3 vec3 = IntVec3.Invalid;

        public override void CompTick()
        {
            base.CompTick();
            Map map = this.parent.Map;
            if (map != null)
            {
                IntVec3 @int = this.parent.Position;
                if ((vec3 == IntVec3.Invalid || (vec3 != IntVec3.Invalid && vec3 != @int)) && Find.TickManager.TicksGame >= this.nextUpdateTick)
                {
                    this.nextUpdateTick = Find.TickManager.TicksGame + 50;
                    map.mapDrawer.MapMeshDirty(this.parent.Position, MapMeshFlagDefOf.Things);
                    map.glowGrid.DeRegisterGlower(this);
                    map.mapDrawer.MapMeshDirty(this.parent.Position, MapMeshFlagDefOf.Things);
                    map.glowGrid.RegisterGlower(this);
                }
                if (Find.TickManager.TicksGame % Props.toxicBuildupEmitTickRate == 0)
                {
                    var pawns = GenRadial.RadialDistinctThingsAround(this.parent.Position, map, Props.toxicBuildupEmitRadius, true).OfType<Pawn>().Where(x => !x.IsGhoul()).ToList();
                    foreach (var pawn in pawns)
                    {
                        float num = Props.toxicBuildupSeverityAdjust;
                        num *= 1f - pawn.GetStatValue(StatDefOf.ToxicResistance);
                        if (num != 0f)
                        {
                            HealthUtility.AdjustSeverity(pawn, HediffDefOf.ToxicBuildup, num);
                        }
                    }
                }

            }

        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            this.nextUpdateTick = Find.TickManager.TicksGame;
            //if (this.parent.Map != null)
            //{
            //    base.PostSpawnSetup(respawningAfterLoad);
            //    if (!respawningAfterLoad)
            //    {
            //        this.nextUpdateTick = Find.TickManager.TicksGame + Rand.Range(0, 100);
            //    }
            //}
        }

        public const int updatePeriodInTicks = 50;

        // Token: 0x04000002 RID: 2
        public int nextUpdateTick;
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<int>(ref this.nextUpdateTick, "nextUpdateTick", 0);
        }
    }
}

