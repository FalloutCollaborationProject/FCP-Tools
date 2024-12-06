using System;
using ArmorRacks.ThingComps;
using ArmorRacks.Things;
using ArmorRacks.Utils;
using RimWorld;
using Verse;

namespace ArmorRacks.Jobs
{
    public abstract class JobDriver_WearRackBase : JobDriver_BaseRackJob
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            AddFailCondition(delegate
            {
                var rack = (ArmorRack) TargetThingA;
                var failText = "ArmorRacks_WearRack_JobFailMessage_RaceMismatch".Translate(pawn.LabelShort);
                foreach (Apparel apparel in rack.GetStoredApparel())
                {
                    if (!ArmorRackJobUtil.RaceCanWear(apparel.def, pawn.kindDef.race))
                    {
                        Messages.Message(failText, MessageTypeDefOf.RejectInput, false);
                        return true;
                    }
                }
                
                foreach (Apparel apparel in pawn.apparel.WornApparel)
                {
                    if (!ArmorRackJobUtil.RaceCanWear(apparel.def, rack.PawnKindDef.race))
                    {
                        Messages.Message(failText, MessageTypeDefOf.RejectInput, false);
                        return true;
                    }
                }

                var rackWeapon = rack.GetStoredWeapon();
                var pawnWeapon = pawn.equipment.Primary;
                var rackCantEquip = rackWeapon != null && !ArmorRackJobUtil.RaceCanEquip(rackWeapon.def, pawn.kindDef.race);
                var pawnCantEquip = pawnWeapon != null && !ArmorRackJobUtil.RaceCanEquip(pawnWeapon.def, rack.PawnKindDef.race);
                if (rackCantEquip || pawnCantEquip)
                {
                    Messages.Message(failText, MessageTypeDefOf.RejectInput, false);
                    return true;
                }
                return false;
            });
            return base.TryMakePreToilReservations(errorOnFailed);
        }
        
        public override int WaitTicks
        {
            get
            {
                var armorRack = (ArmorRack) TargetThingA;
                var pawnTotalEquipDelay = 0f;
                var rackTotalEquipDelay = 0f;
                var rackApparel = armorRack.GetStoredApparel();
                var pawnApparel = pawn.apparel.WornApparel;
                
                foreach (var apparel in rackApparel)
                {
                    var equipDelay = apparel.GetStatValue(StatDefOf.EquipDelay);
                    rackTotalEquipDelay += equipDelay;
                }
                foreach (var apparel in pawnApparel)
                {
                    var equipDelay = apparel.GetStatValue(StatDefOf.EquipDelay);
                    pawnTotalEquipDelay += equipDelay;
                }
                var totalEquipDelay = Math.Max(rackTotalEquipDelay, pawnTotalEquipDelay);

                var armorRackProps = armorRack.GetComp<ArmorRackComp>().Props;
                var powerComp = armorRack.GetComp<CompPowerTrader>();
                var powerOn = powerComp != null && powerComp.PowerOn;
                float equipDelayFactor = powerOn ? armorRackProps.equipDelayFactorPowered : armorRackProps.equipDelayFactor;
                var waitTicks = totalEquipDelay * equipDelayFactor * 60f;
                return (int) waitTicks;
            }
        }
    }
}