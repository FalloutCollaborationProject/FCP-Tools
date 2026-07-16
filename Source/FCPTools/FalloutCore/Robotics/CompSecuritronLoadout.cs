using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace FCP.Core.Robotics
{
    public enum SecuritronWeapon : byte
    {
        Gun,
        GrenadeLauncher
    }

    public class CompProperties_SecuritronLoadout : CompProperties
    {
        public ResearchProjectDef grenadeLauncherResearch;
        public List<ThingDefCountClass> grenadeLauncherCost = new List<ThingDefCountClass>();
        public ResearchProjectDef rocketsResearch;
        public List<ThingDefCountClass> rocketsCost = new List<ThingDefCountClass>();

        public CompProperties_SecuritronLoadout()
        {
            compClass = typeof(CompSecuritronLoadout);
        }
    }

    public class CompSecuritronLoadout : ThingComp
    {
        private SecuritronWeapon weapon = SecuritronWeapon.Gun;
        private bool hasRockets;
        private bool didInitialSetup;

        public CompProperties_SecuritronLoadout Props => (CompProperties_SecuritronLoadout)props;
        public SecuritronWeapon Weapon => weapon;
        public bool HasRockets => hasRockets;

        private Pawn Pawn => parent as Pawn;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (respawningAfterLoad)
            {
                didInitialSetup = true;
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            if (!didInitialSetup)
            {
                didInitialSetup = true;
                DoInitialSetup();
            }
        }

        private void DoInitialSetup()
        {
            SecuritronPresetExtension preset = Pawn?.kindDef.GetModExtension<SecuritronPresetExtension>();
            if (preset != null)
            {
                weapon = preset.weapon;
            }

            EquipWeapon(Pawn, WeaponDefFor(weapon));

            if (preset != null && Rand.Chance(preset.rocketsChance))
            {
                InstallRockets();
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref weapon, "securitronWeapon", SecuritronWeapon.Gun);
            Scribe_Values.Look(ref hasRockets, "securitronHasRockets");
            Scribe_Values.Look(ref didInitialSetup, "securitronLoadoutInitialSetupDone");
        }

        private static ThingDef WeaponDefFor(SecuritronWeapon w)
        {
            return w == SecuritronWeapon.Gun
                ? ThingDefOf_SecuritronLoadout.FCP_Gun_Securitron_Arm
                : ThingDefOf_SecuritronLoadout.FCP_Gun_Securitron_GrenadeLauncher;
        }

        private static void EquipWeapon(Pawn pawn, ThingDef weaponDef)
        {
            if (pawn?.equipment == null)
            {
                return;
            }

            if (pawn.equipment.Primary != null)
            {
                pawn.equipment.DestroyEquipment(pawn.equipment.Primary);
            }

            ThingWithComps newWeapon = (ThingWithComps)ThingMaker.MakeThing(weaponDef);
            CompColorableUtility.SetColor(newWeapon, RobotUtility.GetBodyColor(pawn), reportFailure: false);
            pawn.equipment.AddEquipment(newWeapon);
            RobotUtility.TouchGraphic(newWeapon);
        }

        private void SwitchWeapon(SecuritronWeapon newWeapon)
        {
            Pawn pawn = Pawn;
            if (pawn?.equipment == null || newWeapon == weapon)
            {
                return;
            }

            weapon = newWeapon;
            EquipWeapon(pawn, WeaponDefFor(weapon));
        }

        private void InstallRockets()
        {
            Pawn pawn = Pawn;
            if (pawn?.abilities == null || hasRockets)
            {
                return;
            }

            hasRockets = true;
            pawn.abilities.GainAbility(AbilityDefOf_Securitron.FCP_Ability_Securitron_Rockets);
            if (pawn.apparel != null)
            {
                Apparel rocketPod = (Apparel)ThingMaker.MakeThing(ThingDefOf_SecuritronLoadout.FCP_Apparel_Securitron_RocketPod);
                CompColorableUtility.SetColor(rocketPod, RobotUtility.GetBodyColor(pawn), reportFailure: false);
                pawn.apparel.Wear(rocketPod, dropReplacedApparel: false);
                RobotUtility.TouchGraphic(rocketPod);
            }
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            Pawn pawn = Pawn;
            if (pawn == null || pawn.Faction != Faction.OfPlayer || pawn.CurJobDef != JobDefOf_Robotics.FCP_RobotDock)
            {
                yield break;
            }

            if (!selPawn.CanReach(parent, PathEndMode.Touch, Danger.Deadly))
            {
                yield break;
            }

            if (weapon != SecuritronWeapon.Gun)
            {
                yield return new FloatMenuOption("FCP_SecuritronLoadout_InstallGun".Translate(), delegate
                {
                    SwitchWeapon(SecuritronWeapon.Gun);
                });
            }

            if (weapon != SecuritronWeapon.GrenadeLauncher)
            {
                if (Props.grenadeLauncherResearch != null && !Props.grenadeLauncherResearch.IsFinished)
                {
                    yield return new FloatMenuOption($"{"FCP_SecuritronLoadout_InstallGrenadeLauncher".Translate()}: {"FCP_UpgradeRobot_NeedsResearch".Translate(Props.grenadeLauncherResearch.LabelCap)}", null);
                }
                else if (!RobotUpgradeUtility.CanAffordCost(parent.Map, Props.grenadeLauncherCost))
                {
                    yield return new FloatMenuOption("FCP_SecuritronLoadout_InstallGrenadeLauncher".Translate() + ": " + "FCP_UpgradeRobot_MissingMaterials".Translate(), null);
                }
                else
                {
                    yield return new FloatMenuOption("FCP_SecuritronLoadout_InstallGrenadeLauncher".Translate(), delegate
                    {
                        if (RobotUpgradeUtility.TryConsumeCost(parent.Map, Props.grenadeLauncherCost))
                        {
                            SwitchWeapon(SecuritronWeapon.GrenadeLauncher);
                        }
                    });
                }
            }

            if (!hasRockets)
            {
                if (Props.rocketsResearch != null && !Props.rocketsResearch.IsFinished)
                {
                    yield return new FloatMenuOption($"{"FCP_SecuritronLoadout_InstallRockets".Translate()}: {"FCP_UpgradeRobot_NeedsResearch".Translate(Props.rocketsResearch.LabelCap)}", null);
                }
                else if (!RobotUpgradeUtility.CanAffordCost(parent.Map, Props.rocketsCost))
                {
                    yield return new FloatMenuOption("FCP_SecuritronLoadout_InstallRockets".Translate() + ": " + "FCP_UpgradeRobot_MissingMaterials".Translate(), null);
                }
                else
                {
                    yield return new FloatMenuOption("FCP_SecuritronLoadout_InstallRockets".Translate(), delegate
                    {
                        if (RobotUpgradeUtility.TryConsumeCost(parent.Map, Props.rocketsCost))
                        {
                            InstallRockets();
                        }
                    });
                }
            }
        }
    }
}
