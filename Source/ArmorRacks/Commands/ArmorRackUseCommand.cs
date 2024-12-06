using System.Collections.Generic;
using ArmorRacks.DefOfs;
using ArmorRacks.ThingComps;
using ArmorRacks.Things;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace ArmorRacks.Commands
{
    public class ArmorRackUseCommand : Command
    {
        public ArmorRack ArmorRack;
        public Pawn Pawn;

        public ArmorRackUseCommand(ArmorRack armorRack, Pawn pawn)
        {
            ArmorRack = armorRack;
            Pawn = pawn;
            icon = ContentFinder<Texture2D>.Get(armorRack.def.graphicData.texPath + "_south", false);
            defaultIconColor = armorRack.Stuff.stuffProps.color;
        }

        public override string Label
        {
            get
            {
                var selectedJobDef = Pawn.GetComp<ArmorRackUseCommandComp>().CurArmorRackJobDef;
                if (selectedJobDef == ArmorRacksJobDefOf.ArmorRacks_JobWearRack)
                {
                    return "ArmorRacks_WearRack_FloatMenuLabel".Translate();
                }

                return "ArmorRacks_TransferToRack_FloatMenuLabel".Translate();
            }
        }

        public override string Desc => Label;

        public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions
        {
            get
            {
                // Equip from
                var self = this;
                yield return new FloatMenuOption("ArmorRacks_WearRack_FloatMenuLabel".Translate(),
                    delegate
                    {
                        Pawn.GetComp<ArmorRackUseCommandComp>().CurArmorRackJobDef = ArmorRacksJobDefOf.ArmorRacks_JobWearRack;
                    });
                
                // Transfer to
                yield return new FloatMenuOption("ArmorRacks_TransferToRack_FloatMenuLabel".Translate(),
                    delegate
                    {
                        Pawn.GetComp<ArmorRackUseCommandComp>().CurArmorRackJobDef = ArmorRacksJobDefOf.ArmorRacks_JobTransferToRack;
                    });
            }
        }

        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);
            var target_info = new LocalTargetInfo(ArmorRack);
            var selectedJobDef = Pawn.GetComp<ArmorRackUseCommandComp>().CurArmorRackJobDef;
            var wearRackJob = new Job(selectedJobDef, target_info);
            Pawn.jobs.TryTakeOrderedJob(wearRackJob);
        }
    }
}