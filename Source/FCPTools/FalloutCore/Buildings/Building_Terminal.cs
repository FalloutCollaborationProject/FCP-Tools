using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace FCP.Core.Buildings
{
    public class Building_Terminal : Building_ResearchBench
    {
        private Graphic graphicPoweredOn;
        private Graphic graphicPoweredOff;
        private bool graphicsInitialized;
        private bool holotapeExtracted;
        private CompProperties_Terminal cachedProps;

        private CompProperties_Terminal Props
        {
            get
            {
                if (cachedProps == null)
                    cachedProps = def.GetCompProperties<CompProperties_Terminal>();
                return cachedProps;
            }
        }

        private bool IsPowered
        {
            get
            {
                var powerComp = GetComp<CompPowerTrader>();
                return powerComp == null || powerComp.PowerOn;
            }
        }
        
        public override Graphic Graphic
        {
            get
            {
                if (!graphicsInitialized)
                    InitializeGraphics();
                
                var powerComp = GetComp<CompPowerTrader>();
                if (powerComp != null && powerComp.PowerOn && graphicPoweredOn != null)
                    return graphicPoweredOn;
                
                return graphicPoweredOff ?? base.Graphic;
            }
        }

        private void InitializeGraphics()
        {
            if (graphicsInitialized) 
                return;
            
            // Only load graphics on the main thread
            if (!LongEventHandler.AnyEventNowOrWaiting)
            {
                var props = Props;
                if (props != null)
                {
                    if (!props.poweredOnTexPath.NullOrEmpty())
                    {
                        graphicPoweredOn = GraphicDatabase.Get(
                            def.graphicData.graphicClass,
                            props.poweredOnTexPath,
                            def.graphicData.shaderType.Shader,
                            def.graphicData.drawSize,
                            def.graphicData.color,
                            def.graphicData.colorTwo
                        );
                    }

                    if (!props.poweredOffTexPath.NullOrEmpty())
                    {
                        graphicPoweredOff = GraphicDatabase.Get(
                            def.graphicData.graphicClass,
                            props.poweredOffTexPath,
                            def.graphicData.shaderType.Shader,
                            def.graphicData.drawSize,
                            def.graphicData.color,
                            def.graphicData.colorTwo
                        );
                    }
                }

                graphicsInitialized = true;
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            
            // Defer graphics initialization to main thread if needed
            if (LongEventHandler.AnyEventNowOrWaiting)
            {
                LongEventHandler.ExecuteWhenFinished(() => InitializeGraphics());
            }
            else
            {
                InitializeGraphics();
            }
            
            if (!respawningAfterLoad)
                holotapeExtracted = false;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref holotapeExtracted, "holotapeExtracted", false);
        }

        private bool lastPowerState;

        protected override void Tick()
        {
            base.Tick();
            
            if (this.IsHashIntervalTick(60))
            {
                bool currentPowerState = IsPowered;
                if (currentPowerState != lastPowerState)
                {
                    lastPowerState = currentPowerState;
                    if (Spawned && Map != null)
                        Map.mapDrawer.MapMeshDirty(Position, MapMeshFlagDefOf.Things);
                }
            }
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            var hackComp = GetComp<CompTerminalHacking>();
            if (hackComp != null)
            {
                if (!IsPowered)
                {
                    yield return new FloatMenuOption("Terminal has no power", null);
                    yield break;
                }

                if (hackComp.IsLockedOut)
                {
                    yield return new FloatMenuOption("FCP_TerminalLockedOut_Status".Translate(hackComp.LockoutHoursRemaining), null);
                    yield break;
                }

                if (hackComp.IsLocked)
                {
                    if (!selPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Deadly))
                    {
                        yield return new FloatMenuOption("CannotUseNoPath".Translate(), null);
                        yield break;
                    }

                    if (!selPawn.CanReserve(this))
                    {
                        yield return new FloatMenuOption("CannotUseReserved".Translate(), null);
                        yield break;
                    }

                    yield return new FloatMenuOption("FCP_HackTerminal".Translate(), 
                        () => selPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf_Terminal.FCP_HackTerminal, this)));
                    yield break;
                }
            }

            if (!IsPowered)
            {
                yield return new FloatMenuOption("Cannot use (no power)", null);
                yield break;
            }

            foreach (var option in base.GetFloatMenuOptions(selPawn))
                yield return option;

            var storage = GetComp<CompHolotapeStorage>();
            if (storage != null)
            {
                bool isWallTerminal = hackComp != null;
                
                var carried = selPawn.carryTracker?.CarriedThing;
                if (carried?.TryGetComp<Holotapes.CompHolotape>() != null && !isWallTerminal)
                {
                    if (!selPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Deadly))
                        yield return new FloatMenuOption("CannotUseNoPath".Translate(), null);
                    else if (!selPawn.CanReserve(this))
                        yield return new FloatMenuOption("CannotUseReserved".Translate(), null);
                    else
                        yield return new FloatMenuOption("Insert holotape", () =>
                            selPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf_Terminal.FCP_InsertHolotape, carried, this)));
                }

                if (storage.Count > 0)
                {
                    if (!selPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Deadly))
                        yield return new FloatMenuOption("CannotUseNoPath".Translate(), null);
                    else if (!selPawn.CanReserve(this))
                        yield return new FloatMenuOption("CannotUseReserved".Translate(), null);
                    else
                    {
                        var job = JobMaker.MakeJob(JobDefOf_Terminal.FCP_ReadHolotapeAtTerminal, this);
                        job.targetB = storage.StoredHolotapes[0];
                        yield return new FloatMenuOption("Read holotape", () => selPawn.jobs.TryTakeOrderedJob(job));
                        
                        yield return new FloatMenuOption("Eject holotape", () =>
                        {
                            var holotape = storage.TryRemoveHolotape(storage.StoredHolotapes[0]);
                            if (holotape != null)
                            {
                                GenPlace.TryPlaceThing(holotape, selPawn.Position, selPawn.Map, ThingPlaceMode.Near);
                                Messages.Message($"{selPawn.LabelShort} removed {holotape.Label} from terminal.", 
                                    selPawn, MessageTypeDefOf.NeutralEvent);
                            }
                        });
                    }
                }
                
                var pipboy = selPawn.apparel?.WornApparel?.Find(a => a.TryGetComp<Holotapes.CompPipboyHolotapeStorage>() != null);
                if (pipboy != null && storage != null)
                {
                    if (!selPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Deadly))
                        yield return new FloatMenuOption("CannotUseNoPath".Translate(), null);
                    else if (!selPawn.CanReserve(this))
                        yield return new FloatMenuOption("CannotUseReserved".Translate(), null);
                    else
                        yield return new FloatMenuOption("Link Pip-Boy", () =>
                            selPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf_Terminal.FCP_LinkPipboyToTerminal, this)));
                }
            }

            var props = Props;
            if (props != null && props.canExtractHolotape && props.holotapeDropChance > 0)
            {
                if (holotapeExtracted)
                {
                    yield return new FloatMenuOption("FCP_ExtractHolotape_AlreadyExtracted".Translate(), null);
                }
                else if (!selPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Deadly))
                {
                    yield return new FloatMenuOption("CannotUseNoPath".Translate(), null);
                }
                else if (!selPawn.CanReserve(this))
                {
                    yield return new FloatMenuOption("CannotUseReserved".Translate(), null);
                }
                else
                {
                    yield return new FloatMenuOption("FCP_ExtractHolotape".Translate(), 
                        () => selPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf_Terminal.FCP_ExtractHolotape, this)));
                }
            }
        }

        public void TryExtractHolotape(Pawn extractor)
        {
            if (holotapeExtracted) 
                return;

            holotapeExtracted = true;

            var props = Props;
            if (props == null || !Rand.Chance(props.holotapeDropChance))
            {
                Messages.Message("FCP_NoHolotapeFound".Translate(extractor.Named("PAWN")), 
                    MessageTypeDefOf.NeutralEvent);
                return;
            }

            var techprints = new List<ResearchProjectDef>();
            foreach (var research in DefDatabase<ResearchProjectDef>.AllDefsListForReading)
            {
                if (research.techprintCount > 0 && research.tab?.defName == "FCP_Pre_War_Research")
                    techprints.Add(research);
            }

            if (techprints.Count > 0)
            {
                var project = techprints.RandomElement();
                var holotape = ThingMaker.MakeThing(project.Techprint);
                
                GenPlace.TryPlaceThing(holotape, extractor.Position, extractor.Map, ThingPlaceMode.Near);
                Messages.Message("FCP_ExtractedHolotape".Translate(extractor.Named("PAWN"), holotape.Label), 
                    new LookTargets(holotape), MessageTypeDefOf.PositiveEvent);
            }
            else
            {
                Messages.Message("FCP_NoHolotapeFound".Translate(extractor.Named("PAWN")), 
                    MessageTypeDefOf.NeutralEvent);
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
                yield return gizmo;

            if (Prefs.DevMode && Props != null && Props.canExtractHolotape)
            {
                yield return new Command_Action
                {
                    defaultLabel = "DEV: Reset holotape",
                    action = () => holotapeExtracted = false
                };
            }
        }

        public override string GetInspectString()
        {
            var text = base.GetInspectString();
            
            var hackComp = GetComp<CompTerminalHacking>();
            if (hackComp != null)
            {
                if (hackComp.IsLockedOut)
                {
                    if (!text.NullOrEmpty())
                        text += "\n";
                    text += "FCP_TerminalLockedOut_Status".Translate(hackComp.LockoutHoursRemaining);
                }
                else if (hackComp.IsLocked)
                {
                    if (!text.NullOrEmpty())
                        text += "\n";
                    text += "FCP_TerminalLocked".Translate();
                }
            }

            var props = Props;
            if (props != null && props.canExtractHolotape)
            {
                if (!text.NullOrEmpty())
                    text += "\n";
                
                text += holotapeExtracted 
                    ? "FCP_HolotapeStatus_Extracted".Translate() 
                    : "FCP_HolotapeStatus_CanExtract".Translate();
            }

            return text;
        }
    }
}
