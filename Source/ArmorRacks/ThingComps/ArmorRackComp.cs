using System;
using System.Collections.Generic;
using System.Linq;
using ArmorRacks.Commands;
using ArmorRacks.DefOfs;
using ArmorRacks.Things;
using ArmorRacks.Utils;
using RimWorld;
using Verse;
using Verse.AI;

namespace ArmorRacks.ThingComps
{
    public class ArmorRackCompProperties : CompProperties
    {
        public float equipDelayFactor;
        public float equipDelayFactorPowered;

        public ArmorRackCompProperties()
        {
            this.compClass = typeof(ArmorRackComp);
        }

        public ArmorRackCompProperties(Type compClass) : base(compClass)
        {
            this.compClass = compClass;
        }
    }

    public class ArmorRackComp : ThingComp
    {
        public ArmorRackCompProperties Props => (ArmorRackCompProperties) this.props;

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            ArmorRack armorRack = this.parent as ArmorRack;
            
            if (!selPawn.CanReach(armorRack, PathEndMode.Touch, Danger.Deadly, false, false, TraverseMode.ByPawn))
            {
                FloatMenuOption failer = new FloatMenuOption("CannotUseNoPath".Translate(), null, MenuOptionPriority.Default, null, null, 0f, null, null);
                yield return failer;
                yield break;
            }
            
            var nonViolentOptionYielded = false;
            if (ArmorRackJobUtil.PawnCanEquipWeaponSet(armorRack, selPawn))
            {
                // Transfer to
                var swapWithOption = new FloatMenuOption("ArmorRacks_TransferToRack_FloatMenuLabel".Translate(), delegate
                {
                    var target_info = new LocalTargetInfo(armorRack);
                    var wearRackJob = new Job(ArmorRacksJobDefOf.ArmorRacks_JobTransferToRack, target_info);
                    selPawn.jobs.TryTakeOrderedJob(wearRackJob);
                });
                yield return FloatMenuUtility.DecoratePrioritizedTask(swapWithOption, selPawn, armorRack, "ReservedBy");
            }
            else
            {
                yield return new FloatMenuOption("ArmorRacks_WearRack_FloatMenuLabel_NonViolent".Translate(), null);
                nonViolentOptionYielded = true;
            }
            
            if (ForbidUtility.IsForbidden(armorRack, selPawn))
            {
                yield break;
            }

            if (ArmorRackJobUtil.RackHasItems(armorRack))
            {
                if (ArmorRackJobUtil.PawnCanEquipWeaponSet(armorRack, selPawn))
                {
                    // Equip from
                    var equipFromOption = new FloatMenuOption("ArmorRacks_WearRack_FloatMenuLabel".Translate(), delegate
                    {
                        var target_info = new LocalTargetInfo(armorRack);
                        var wearRackJob = new Job(ArmorRacksJobDefOf.ArmorRacks_JobWearRack, target_info);
                        selPawn.jobs.TryTakeOrderedJob(wearRackJob);
                    });
                    yield return FloatMenuUtility.DecoratePrioritizedTask(equipFromOption, selPawn, armorRack, "ReservedBy");
                }
                else if (!nonViolentOptionYielded)
                {
                    yield return new FloatMenuOption("ArmorRacks_WearRack_FloatMenuLabel_NonViolent".Translate(), null);
                }
            }
            else
            {
                yield return new FloatMenuOption("ArmorRacks_WearRack_FloatMenuLabel_Empty".Translate(), null);
            }
            
        }

        public override void PostDraw()
        {
            ArmorRack armorRack = this.parent as ArmorRack;
            armorRack.ContentsDrawer.DrawAt(armorRack.DrawPos);
        }
    }

}