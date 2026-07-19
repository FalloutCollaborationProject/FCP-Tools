using System.Collections.Generic;
using System.Linq;
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
            Pawn pawn = Pawn;
            if (pawn == null)
            {
                return;
            }

            SecuritronPresetExtension preset = pawn.kindDef.GetModExtension<SecuritronPresetExtension>();
            if (preset != null)
            {
                weapon = preset.weapon;
            }

            pawn.SetColor(RobotUtility.GetBodyColor(pawn));
            RobotUtility.TouchBodyColor(pawn);

            EquipWeapon(pawn, WeaponHediffFor(weapon));

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

        private static HediffDef WeaponHediffFor(SecuritronWeapon w)
        {
            return w == SecuritronWeapon.Gun
                ? HediffDefOf_Securitron.FCP_Hediff_Securitron_Gun
                : HediffDefOf_Securitron.FCP_Hediff_Securitron_GrenadeLauncher;
        }

        private static BodyPartRecord FindWeaponMount(Pawn pawn)
        {
            return pawn.RaceProps.body.AllParts.FirstOrDefault(p => p.groups.Contains(BodyPartGroupDefOf_Securitron.SecuritronWeaponMount));
        }

        private static void EquipWeapon(Pawn pawn, HediffDef weaponDef)
        {
            if (pawn?.health == null)
            {
                return;
            }

            BodyPartRecord part = FindWeaponMount(pawn);
            if (part == null)
            {
                return;
            }

            Hediff existing = pawn.health.hediffSet.hediffs.FirstOrDefault(h => h.Part == part && h.def.GetModExtension<RobotHediffGraphic>() != null);
            if (existing != null)
            {
                if (existing.def == weaponDef)
                {
                    return;
                }
                pawn.health.RemoveHediff(existing);
            }
            pawn.health.AddHediff(weaponDef, part);
            pawn.Drawer.renderer.SetAllGraphicsDirty();
        }

        private void SwitchWeapon(SecuritronWeapon newWeapon)
        {
            Pawn pawn = Pawn;
            if (pawn?.health == null || newWeapon == weapon)
            {
                return;
            }

            weapon = newWeapon;
            EquipWeapon(pawn, WeaponHediffFor(weapon));
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

            BodyPartRecord part = pawn.RaceProps.body.AllParts.FirstOrDefault(p => p.groups.Contains(BodyPartGroupDefOf_Securitron.SecuritronShoulder));
            if (part != null)
            {
                pawn.health.AddHediff(HediffDefOf_Securitron.FCP_Hediff_Securitron_RocketPod, part);
                pawn.Drawer.renderer.SetAllGraphicsDirty();
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

            yield return new FloatMenuOption("FCP_RobotUpgradeDialog_Open".Translate(), delegate
            {
                Find.WindowStack.Add(new Dialog_RobotUpgrade(pawn, BuildUpgradeOptions));
            });
        }

        private List<RobotUpgradeOption> BuildUpgradeOptions()
        {
            List<RobotUpgradeOption> options = new List<RobotUpgradeOption>();

            if (weapon != SecuritronWeapon.Gun)
            {
                options.Add(new RobotUpgradeOption
                {
                    category = "Weapon",
                    label = "FCP_SecuritronLoadout_InstallGun".Translate(),
                    install = () => SwitchWeapon(SecuritronWeapon.Gun),
                });
            }

            if (weapon != SecuritronWeapon.GrenadeLauncher)
            {
                if (Props.grenadeLauncherResearch != null && !Props.grenadeLauncherResearch.IsFinished)
                {
                    options.Add(new RobotUpgradeOption
                    {
                        category = "Weapon",
                        label = "FCP_SecuritronLoadout_InstallGrenadeLauncher".Translate(),
                        disabledReason = "FCP_UpgradeRobot_NeedsResearch".Translate(Props.grenadeLauncherResearch.LabelCap),
                    });
                }
                else
                {
                    List<ThingDefCountClass> cost = Props.grenadeLauncherCost;
                    bool afford = RobotUpgradeUtility.CanAffordCost(parent.Map, cost);
                    options.Add(new RobotUpgradeOption
                    {
                        category = "Weapon",
                        label = "FCP_SecuritronLoadout_InstallGrenadeLauncher".Translate(),
                        cost = cost,
                        disabledReason = afford ? null : "FCP_UpgradeRobot_MissingMaterials".Translate(),
                        install = delegate
                        {
                            if (RobotUpgradeUtility.TryConsumeCost(parent.Map, cost))
                            {
                                SwitchWeapon(SecuritronWeapon.GrenadeLauncher);
                            }
                        },
                    });
                }
            }

            if (!hasRockets)
            {
                if (Props.rocketsResearch != null && !Props.rocketsResearch.IsFinished)
                {
                    options.Add(new RobotUpgradeOption
                    {
                        category = "Rockets",
                        label = "FCP_SecuritronLoadout_InstallRockets".Translate(),
                        disabledReason = "FCP_UpgradeRobot_NeedsResearch".Translate(Props.rocketsResearch.LabelCap),
                    });
                }
                else
                {
                    List<ThingDefCountClass> cost = Props.rocketsCost;
                    bool afford = RobotUpgradeUtility.CanAffordCost(parent.Map, cost);
                    options.Add(new RobotUpgradeOption
                    {
                        category = "Rockets",
                        label = "FCP_SecuritronLoadout_InstallRockets".Translate(),
                        cost = cost,
                        disabledReason = afford ? null : "FCP_UpgradeRobot_MissingMaterials".Translate(),
                        install = delegate
                        {
                            if (RobotUpgradeUtility.TryConsumeCost(parent.Map, cost))
                            {
                                InstallRockets();
                            }
                        },
                    });
                }
            }

            return options;
        }
    }
}
