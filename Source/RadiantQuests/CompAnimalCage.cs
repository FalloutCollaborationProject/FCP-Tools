using PipeSystem;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Diagnostics;
using Verse;
using Verse.AI;

namespace FCP_RadiantQuests
{
    [StaticConstructorOnStartup]
    public class CompAnimalCage : CompRefuelable, IThingHolder, ISuspendableThingHolder, IStoreSettingsParent
    {

        protected bool contentsKnown;

        public CompResource compResourceNutrientPaste;

        public Job queuedEnterJob;

        public Pawn queuedPawn;

        public bool pawnStarving = false;
        private bool shouldCapture = true;
        public bool ShouldCapture
        {
            get { return shouldCapture; }
            set
            {
                if (value == shouldCapture)
                {
                    return;
                }
                shouldCapture = value;
            }
        }

        public int captureTicks = 600;

        public int starvingCounter = 0;

        public const int starvingMaxTicks = 175000;

        public CompFlickable flickComp;

        public ThingOwner innerContainer;

        public StorageSettings allowedNutritionSettings;

        public Pawn Occupant => innerContainer.OfType<Pawn>().FirstOrDefault();

        public bool IsContentsSuspended => true;

        public bool StorageTabVisible => true;

        private float ConsumptionRatePerTick => base.Props.fuelConsumptionRate / 60000f;

        public ThingFilter FuelFilter => GetStoreSettings().filter;

        public new CompProperties_AnimalCage Props => (CompProperties_AnimalCage)props;

        public CompAnimalCage()
        {
            innerContainer = new ThingOwner<Thing>(this);
        }


        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return innerContainer;
        }

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            flickComp = parent.GetComp<CompFlickable>();
            foreach (CompResource comp in parent.GetComps<CompResource>())
            {

                if (ModLister.HasActiveModWithName("Vanilla Nutrient Paste Expanded") && ((comp == null) ? null : ((Def)(object)comp.Props?.pipeNet)?.defName) == "VNPE_NutrientPasteNet")
                {
                    compResourceNutrientPaste = comp;
                }
            }
            allowedNutritionSettings = new StorageSettings(this);
            if (parent.def.building.defaultStorageSettings != null)
            {
                allowedNutritionSettings.CopyFrom(parent.def.building.defaultStorageSettings);
            }
        }

        public override void CompTick()
        {
            if (!base.Props.consumeFuelOnlyWhenUsed && (flickComp == null || flickComp.SwitchIsOn) && Occupant != null)
            {
                ConsumeFuel(ConsumptionRatePerTick);
            }

            if (ShouldCapture && parent.IsHashIntervalTick(Props.ticksForCaptureChance) && Occupant == null && Fuel != 0 && Rand.Chance(Props.captureChance))
            {
                CaptureAnimal();
            }
            if (parent.IsHashIntervalTick(6000))
            {
                if (base.Fuel == 0f)
                {
                    pawnStarving = true;
                }
                else
                {
                    pawnStarving = false;
                    starvingCounter = 0;
                }
            }
            if (pawnStarving)
            {
                starvingCounter++;
                if (starvingCounter > 175000)
                {
                    EjectAndKillContents(parent.Map);
                }
            }
            if (parent.IsHashIntervalTick(600) && compResourceNutrientPaste != null && compResourceNutrientPaste.PipeNet.Stored > 1f && base.FuelPercentOfMax < 0.5f)
            {
                compResourceNutrientPaste.PipeNet.DrawAmongStorage(1f, compResourceNutrientPaste.PipeNet.storages);
                Refuel(18f);
            }
        }

        public StorageSettings GetStoreSettings()
        {
            return allowedNutritionSettings;
        }

        public StorageSettings GetParentStoreSettings()
        {
            return parent.def.building.fixedStorageSettings;
        }

        public void Notify_SettingsChanged()
        {
        }

        public void CaptureAnimal()
        {

            Log.Message(ShouldCapture);
            if (Props.animalsThatGetCaught == null)
            {
                Log.Message("animals that get caught list is empty");
                return;
            }
            foreach (var item in parent.Map.Biome.AllWildAnimals)
            {
                Log.Message(item.defName);
            }
            Log.Message("Props animals");
            foreach (var item in Props.animalsThatGetCaught)
            {
                Log.Message(item.defName);
            }
            if (!parent.Map.Biome.AllWildAnimals.Any(c => Props.animalsThatGetCaught.Any(x => c.defName == x.defName)))
            {
                Log.Message("no animals catchable that exist in this biome");
                return;
            }
            Log.Message("Generating and inserting animal");
            PawnKindDef def = parent.Map.Biome.AllWildAnimals.Where(c => Props.animalsThatGetCaught.Any(x => c == x)).RandomElement();
            Pawn pawn = PawnGenerator.GeneratePawn(def);
            Letter letter = LetterMaker.MakeLetter("FCP_CageAnimalCapturedLabel".Translate(), "FCP_CageAnimalCapturedText".Translate(pawn), LetterDefOf.PositiveEvent, base.parent);
            Find.LetterStack.ReceiveLetter(letter);
            this.InsertPawn(pawn);
        }
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
        }

        public override void PostDeSpawn(Map map)
        {
            EjectContents(map);
            base.PostDeSpawn(map);
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            EjectContents(previousMap);
            base.PostDestroy(mode, previousMap);
        }

        public bool InsertPawn(Pawn pawn)
        {
            return innerContainer.TryAddOrTransfer(pawn, canMergeWithExistingStacks: false);
        }

        public bool Accepts(Thing thing)
        {
            return innerContainer.CanAcceptAnyOf(thing);
        }

        public bool TryAcceptPawn(Pawn pawn)
        {
            innerContainer.ClearAndDestroyContents();
            bool flag = pawn.DeSpawnOrDeselect();
            if (pawn.holdingOwner != null)
            {
                pawn.holdingOwner.TryTransferToContainer(pawn, innerContainer);
            }
            else
            {
                innerContainer.TryAdd(pawn);
            }
            if (flag)
            {
                Find.Selector.Select(pawn, playSound: false, forceDesignatorDeselect: false);
            }
            return true;
        }

        public virtual bool CanAcceptPawn(Pawn pawn)
        {
            return Occupant == null;
        }


        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref contentsKnown, "contentsKnown", defaultValue: false);
            Scribe_Values.Look(ref pawnStarving, "pawnStarving", defaultValue: false);
            Scribe_Values.Look(ref starvingCounter, "starvingCounter", 0);
            Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
            Scribe_References.Look(ref queuedEnterJob, "queuedEnterJob");
            Scribe_References.Look(ref queuedPawn, "queuedPawn");
            Scribe_Deep.Look(ref allowedNutritionSettings, "allowedNutritionSettings");
        }

        public override string CompInspectStringExtra()
        {
            return base.CompInspectStringExtra();
        }


        public override void PostDraw()
        {
            base.PostDraw();
            if (Occupant != null)
            {
                Vector3 drawPos = parent.DrawPos;
                drawPos.y += 10f;
                Occupant.Drawer.renderer.DynamicDrawPhaseAt(DrawPhase.Draw, drawPos, null, neverAimWeapon: true);
            }
        }
        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            foreach (FloatMenuOption item in base.CompFloatMenuOptions(selPawn))
            {
                yield return item;
            }

            if (!selPawn.CanReach(parent, PathEndMode.InteractionCell, Danger.Deadly))
            {
                yield return new FloatMenuOption("CannotUseNoPath".Translate(), null);
                yield break;
            }
            foreach (Thing item in innerContainer)
            {
                if (item is Pawn pawn)
                {
                    JobDef jobDef = DefOfs.FCP_ReleaseAnimalFromCage;
                    string label = "FCP_ReleaseAnimalFromCage".Translate(pawn.Label);
                    Action action = delegate
                    {
                        Job job = JobMaker.MakeJob(jobDef, parent);
                        selPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);


                    };
                    yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label, action), selPawn, parent);


                    string label1 = "FCP_TransferAnimalBetweenCages".Translate(pawn.Label);
                    Action action1 = delegate
                    {
                        AnimalCageUtility.TargetCageForAnimal(selPawn, pawn, true, parent);

                    };
                    yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label1, action1), selPawn, parent);
                    
                }
            }

        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo item in base.CompGetGizmosExtra())
            {
                yield return item;
            }
            Command_Toggle command_Toggle = new Command_Toggle();
            command_Toggle.icon = null;
            command_Toggle.isActive = () => ShouldCapture;
            command_Toggle.defaultLabel = "FCP_ShouldCaptureCommand".Translate();
            command_Toggle.activateIfAmbiguous = false;
            command_Toggle.toggleAction = delegate
            {
                ShouldCapture = !ShouldCapture;
            };
            if (shouldCapture)
            {
                command_Toggle.defaultDesc = "FCP_ShouldCaptureTrue".Translate();
            }
            else
            {
                command_Toggle.defaultDesc = "FCP_ShouldCaptureFalse".Translate();
            }
            yield return command_Toggle;
            if (innerContainer.Count <= 0)
            {
                yield break;
            }
            using (IEnumerator<Thing> enumerator2 = ((IEnumerable<Thing>)innerContainer).GetEnumerator())
            {
                while (enumerator2.MoveNext())
                {
                    Gizmo gizmo = Building.SelectContainedItemGizmo(item: enumerator2.Current, container: parent);
                    if (gizmo != null)
                    {
                        yield return gizmo;
                    }
                }
            }

        }

        public void EjectContents(Map map)
        {
            foreach (Thing item in (IEnumerable<Thing>)innerContainer)
            {
                if (item is Pawn pawn)
                {
                    PawnComponentsUtility.AddComponentsForSpawn(pawn);

                }
            }
            innerContainer.TryDropAll(parent.InteractionCell, map, ThingPlaceMode.Near);
            contentsKnown = true;
        }

        public void EjectAndKillContents(Map map)
        {
            ThingDef filth_Slime = ThingDefOf.Filth_Slime;
            foreach (Thing item in (IEnumerable<Thing>)innerContainer)
            {
                if (item is Pawn pawn)
                {
                    PawnComponentsUtility.AddComponentsForSpawn(pawn);
                    pawn.filth.GainFilth(filth_Slime);
                    pawn.Kill(null, null);
                }
            }
            innerContainer.TryDropAll(parent.InteractionCell, map, ThingPlaceMode.Near);
            contentsKnown = true;
        }
        public static void AddCarryToCageJobs(List<FloatMenuOption> opts, Pawn pawn, Pawn target)
        {
            if (!pawn.CanReserveAndReach(target, PathEndMode.OnCell, Danger.Deadly, 1, -1, null, ignoreOtherReservations: true))
            {
                return;
            }
            string text = "";
            Action action = null;
            if (!AnimalCageUtility.TryGetCagesThatFitBodySize(target.RaceProps.baseBodySize, target.Map, out List<CompAnimalCage> cages))
            {
                return;
            }
            foreach (CompAnimalCage item in cages)
            {
                text = "FCP_CarryAnimalToCage".Translate(target);
                if (target.IsQuestLodger())
                {
                    text += " (" + "CryptosleepCasketGuestsNotAllowed".Translate() + ")";
                    continue;
                }
                if (!item.CanAcceptPawn(target))
                {
                    text += " (" + "CryptosleepCasketOccupied".Translate() + ")";
                    continue;
                }
                Thing pod = item.parent;
                action = delegate
                {
                    Job job = JobMaker.MakeJob(DefOfs.FCP_CarryAnimalToCage, target, pod);
                    job.count = 1;
                    pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                };
                break;
            }
            if (!text.NullOrEmpty())
            {
                opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(text, action), pawn, target));
            }
        }
    }
}
