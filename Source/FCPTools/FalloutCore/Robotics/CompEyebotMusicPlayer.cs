using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace FCP.Core.Robotics
{
    public class CompProperties_EyebotMusicPlayer : CompProperties
    {
        public CompProperties_EyebotMusicPlayer()
        {
            compClass = typeof(CompEyebotMusicPlayer);
        }
    }

    public class CompEyebotMusicPlayer : ThingComp
    {
        public ThingDef LoadedTapeDef;
        public int NextJoyTick;
        private Sustainer sustainer;

        public CompProperties_EyebotMusicPlayer Props => (CompProperties_EyebotMusicPlayer)props;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Defs.Look(ref LoadedTapeDef, "loadedMusicTapeDef");
            Scribe_Values.Look(ref NextJoyTick, "nextMusicJoyTick");
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (respawningAfterLoad || LoadedTapeDef != null)
            {
                return;
            }

            Pawn eyebot = parent as Pawn;
            string factionDefName = eyebot?.Faction?.def.defName;
            if (factionDefName == null)
            {
                return;
            }

            ThingDef broadcastTape = DefDatabase<ThingDef>.GetNamedSilentFail("FCP_Item_Music_Holotape_" + factionDefName + "_Broadcast");
            if (broadcastTape == null)
            {
                return;
            }

            LoadedTapeDef = broadcastTape;
            eyebot.GetComp<CompEyebotMode>()?.SetMode(EyebotMode.Music);
        }

        public override void CompTickRare()
        {
            base.CompTickRare();
            UpdateSustainer();
        }

        private void UpdateSustainer()
        {
            SoundDef track = LoadedTapeDef?.GetCompProperties<CompProperties_MusicHolotape>()?.trackToPlay;
            bool shouldPlay = track != null && parent.Spawned
                && parent is Pawn eyebot
                && eyebot.GetComp<CompEyebotMode>()?.Mode == EyebotMode.Music
                && (eyebot.GetComp<CompRefuelable>()?.HasFuel ?? true);

            if (shouldPlay)
            {
                if (sustainer == null || sustainer.Ended || sustainer.def != track)
                {
                    sustainer?.End();
                    sustainer = track.TrySpawnSustainer(SoundInfo.InMap(parent, MaintenanceType.PerTickRare));
                }
                else
                {
                    sustainer.Maintain();
                }
            }
            else if (sustainer != null && !sustainer.Ended)
            {
                sustainer.End();
            }
        }

        public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
        {
            base.PostDeSpawn(map, mode);
            if (sustainer != null && !sustainer.Ended)
            {
                sustainer.End();
            }
        }

        public override string CompInspectStringExtra()
        {
            return LoadedTapeDef == null
                ? "FCP_EyebotMusic_NoTape".Translate()
                : "FCP_EyebotMusic_Loaded".Translate(LoadedTapeDef.LabelCap);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (LoadedTapeDef == null)
            {
                yield return new Command_Action
                {
                    defaultLabel = "FCP_LoadMusicHolotape_Gizmo".Translate(),
                    defaultDesc = "FCP_LoadMusicHolotape_GizmoDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("Things/Items/Techprints/FCP_Techprint_Holotape_Orange"),
                    action = OpenLoadTapeMenu,
                };
            }
            else
            {
                yield return new Command_Action
                {
                    defaultLabel = "FCP_EjectMusicHolotape_Gizmo".Translate(),
                    defaultDesc = "FCP_EjectMusicHolotape_GizmoDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("Things/Items/Techprints/FCP_Techprint_Holotape_Orange"),
                    action = EjectTape,
                };
            }
        }

        private void OpenLoadTapeMenu()
        {
            if (parent.Map == null)
            {
                return;
            }

            List<Thing> tapes = parent.Map.listerThings.AllThings
                .Where(t => t.def.HasComp(typeof(CompMusicHolotape)))
                .ToList();

            if (tapes.Count == 0)
            {
                Messages.Message("FCP_LoadMusicHolotape_None".Translate(), MessageTypeDefOf.RejectInput, historical: false);
                return;
            }

            List<FloatMenuOption> options = tapes
                .Select(tape => new FloatMenuOption(tape.LabelCap, () => LoadTape(tape)))
                .ToList();

            Find.WindowStack.Add(new FloatMenu(options));
        }

        private void LoadTape(Thing tape)
        {
            LoadedTapeDef = tape.def;
            tape.SplitOff(1).Destroy();
        }

        private void EjectTape()
        {
            if (LoadedTapeDef == null || parent.Map == null)
            {
                return;
            }

            GenPlace.TryPlaceThing(ThingMaker.MakeThing(LoadedTapeDef), parent.Position, parent.Map, ThingPlaceMode.Near);
            LoadedTapeDef = null;
        }
    }
}
