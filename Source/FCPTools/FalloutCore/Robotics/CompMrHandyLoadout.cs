using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace FCP.Core.Robotics
{
    public class CompProperties_MrHandyLoadout : CompProperties
    {
        public List<ThingDefCountClass> toolSwapCost = new List<ThingDefCountClass>();
        public List<ThingDefCountClass> armorEyesCost = new List<ThingDefCountClass>();
        public List<ThingDefCountClass> armorBodyCost = new List<ThingDefCountClass>();
        public List<ThingDefCountClass> repairPartCost = new List<ThingDefCountClass>();
        public ResearchProjectDef gunResearch;
        public List<ThingDefCountClass> gunCost = new List<ThingDefCountClass>();
        public ResearchProjectDef laserResearch;
        public List<ThingDefCountClass> laserCost = new List<ThingDefCountClass>();
        public ResearchProjectDef plasmaResearch;
        public List<ThingDefCountClass> plasmaCost = new List<ThingDefCountClass>();

        public CompProperties_MrHandyLoadout()
        {
            compClass = typeof(CompMrHandyLoadout);
        }
    }

    public class CompMrHandyLoadout : ThingComp
    {
        private static readonly Color DefaultColor = new Color32(200, 200, 200, 255);

        private static readonly BodyPartGroupDef[] RepairableGroups =
        {
            BodyPartGroupDefOf_MrHandy.MrHandyArmLeft,
            BodyPartGroupDefOf_MrHandy.MrHandyArmCenter,
            BodyPartGroupDefOf_MrHandy.MrHandyArmRight,
            BodyPartGroupDefOf_MrHandy.MrHandyEyeLeft,
            BodyPartGroupDefOf_MrHandy.MrHandyEyeCenter,
            BodyPartGroupDefOf_MrHandy.MrHandyEyeRight,
            BodyPartGroupDefOf_MrHandy.MrHandyThruster,
        };

        private bool didInitialSetup;
        private MrHandyRole role;

        public CompProperties_MrHandyLoadout Props => (CompProperties_MrHandyLoadout)props;

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

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref didInitialSetup, "mrHandyLoadoutInitialSetupDone");
            Scribe_Values.Look(ref role, "mrHandyRole", MrHandyRole.Handy);
        }

        private void DoInitialSetup()
        {
            Pawn pawn = Pawn;
            if (pawn?.health == null)
            {
                return;
            }

            MrHandyPresetExtension preset = pawn.kindDef.GetModExtension<MrHandyPresetExtension>();
            role = preset?.role ?? MrHandyRole.Handy;

            InstallTool(pawn, BodyPartGroupDefOf_MrHandy.MrHandyArmLeft, HediffDefOf_MrHandy.FCP_Hediff_MrHandy_ArmStalk_Left);
            InstallTool(pawn, BodyPartGroupDefOf_MrHandy.MrHandyArmCenter, HediffDefOf_MrHandy.FCP_Hediff_MrHandy_ArmStalk_Center);
            InstallTool(pawn, BodyPartGroupDefOf_MrHandy.MrHandyArmRight, HediffDefOf_MrHandy.FCP_Hediff_MrHandy_ArmStalk_Right);
            InstallTool(pawn, BodyPartGroupDefOf_MrHandy.MrHandyArmLeft, preset?.leftTool);
            InstallTool(pawn, BodyPartGroupDefOf_MrHandy.MrHandyArmCenter, preset?.centerTool);
            InstallTool(pawn, BodyPartGroupDefOf_MrHandy.MrHandyArmRight, preset?.rightTool);
            InstallTool(pawn, BodyPartGroupDefOf_MrHandy.MrHandyThruster, HediffDefOf_MrHandy.FCP_Hediff_MrHandy_Thruster);
            InstallTool(pawn, BodyPartGroupDefOf_MrHandy.MrHandyEyeLeft, HediffDefOf_MrHandy.FCP_Hediff_MrHandy_Eye_Left);
            InstallTool(pawn, BodyPartGroupDefOf_MrHandy.MrHandyEyeCenter, HediffDefOf_MrHandy.FCP_Hediff_MrHandy_Eye_Center);
            InstallTool(pawn, BodyPartGroupDefOf_MrHandy.MrHandyEyeRight, HediffDefOf_MrHandy.FCP_Hediff_MrHandy_Eye_Right);

            pawn.SetColor(preset?.color ?? DefaultColor);
            RobotUtility.TouchBodyColor(pawn);
            TouchHediffGraphic(pawn, HediffDefOf_MrHandy.FCP_Hediff_MrHandy_ArmStalk_Left);
            TouchHediffGraphic(pawn, HediffDefOf_MrHandy.FCP_Hediff_MrHandy_ArmStalk_Center);
            TouchHediffGraphic(pawn, HediffDefOf_MrHandy.FCP_Hediff_MrHandy_ArmStalk_Right);
            TouchHediffGraphic(pawn, HediffDefOf_MrHandy.FCP_Hediff_MrHandy_Eye_Left);
            TouchHediffGraphic(pawn, HediffDefOf_MrHandy.FCP_Hediff_MrHandy_Eye_Center);
            TouchHediffGraphic(pawn, HediffDefOf_MrHandy.FCP_Hediff_MrHandy_Eye_Right);
            pawn.Drawer.renderer.SetAllGraphicsDirty();
        }

        private static void TouchHediffGraphic(Pawn pawn, HediffDef hediffDef)
        {
            Graphic graphic = hediffDef != null ? RobotHediffGraphicCache.GetFor(hediffDef) : null;
            CompColorable colorable = pawn.GetComp<CompColorable>();
            if (graphic != null && colorable != null && colorable.Active)
            {
                _ = graphic.GetColoredVersion(graphic.Shader, colorable.Color, graphic.ColorTwo);
            }
        }

        private static void InstallTool(Pawn pawn, BodyPartGroupDef group, HediffDef hediffDef)
        {
            if (hediffDef == null || group == null)
            {
                return;
            }

            BodyPartRecord part = FindPart(pawn, group);
            if (part == null || pawn.health.hediffSet.hediffs.Any(h => h.def == hediffDef && h.Part == part))
            {
                return;
            }

            pawn.health.AddHediff(hediffDef, part);
        }

        private static BodyPartRecord FindPart(Pawn pawn, BodyPartGroupDef group)
        {
            return pawn.RaceProps.body.AllParts.FirstOrDefault(p => p.groups.Contains(group));
        }

        private static HediffDef CurrentVariant(Pawn pawn, BodyPartGroupDef group, bool isBaseArm = false)
        {
            BodyPartRecord part = FindPart(pawn, group);
            if (part == null)
            {
                return null;
            }
            return pawn.health.hediffSet.hediffs.FirstOrDefault(h =>
                h.Part == part && h.def.GetModExtension<RobotHediffGraphic>() is RobotHediffGraphic rhg && rhg.isBaseArm == isBaseArm)?.def;
        }

        private static void SwapVariant(Pawn pawn, BodyPartGroupDef group, HediffDef newHediff, bool isBaseArm = false)
        {
            BodyPartRecord part = FindPart(pawn, group);
            if (part == null || newHediff == null)
            {
                return;
            }

            Hediff existing = pawn.health.hediffSet.hediffs.FirstOrDefault(h =>
                h.Part == part && h.def.GetModExtension<RobotHediffGraphic>() is RobotHediffGraphic rhg && rhg.isBaseArm == isBaseArm);
            if (existing != null)
            {
                if (existing.def == newHediff)
                {
                    return;
                }
                pawn.health.RemoveHediff(existing);
            }

            pawn.health.AddHediff(newHediff, part);
            TouchHediffGraphic(pawn, newHediff);
            pawn.Drawer.renderer.SetAllGraphicsDirty();
        }

        public bool SupportsMode(MrHandyMode mode)
        {
            Pawn pawn = Pawn;
            if (pawn?.health?.hediffSet == null)
            {
                return false;
            }

            switch (mode)
            {
                case MrHandyMode.Wander:
                    return true;
                case MrHandyMode.Guard:
                case MrHandyMode.GuardPawn:
                    return role == MrHandyRole.Gutsy;
                case MrHandyMode.Cook:
                case MrHandyMode.Clean:
                case MrHandyMode.Garden:
                    return role == MrHandyRole.Handy && HasHediffOnGroup(pawn, BodyPartGroupDefOf_MrHandy.MrHandyArmLeft, HediffDefOf_MrHandy.FCP_Hediff_MrHandy_Left_Saw);
                case MrHandyMode.BasicCare:
                case MrHandyMode.Tend:
                    return role == MrHandyRole.Orderly && HasHediffOnGroup(pawn, BodyPartGroupDefOf_MrHandy.MrHandyArmRight, HediffDefOf_MrHandy.FCP_Hediff_MrHandy_Right_Pincer);
                case MrHandyMode.Childcare:
                    return role == MrHandyRole.Nanny && HasHediffOnGroup(pawn, BodyPartGroupDefOf_MrHandy.MrHandyArmRight, HediffDefOf_MrHandy.FCP_Hediff_MrHandy_Right_Pincer);
                default:
                    return false;
            }
        }

        private static bool HasHediffOnGroup(Pawn pawn, BodyPartGroupDef group, HediffDef hediffDef)
        {
            BodyPartRecord part = FindPart(pawn, group);
            return part != null && !pawn.health.hediffSet.PartIsMissing(part)
                && pawn.health.hediffSet.hediffs.Any(h => h.def == hediffDef && h.Part == part);
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
                Find.WindowStack.Add(new Dialog_RobotUpgrade(pawn, () => BuildUpgradeOptions(pawn)));
            });
        }

        private List<RobotUpgradeOption> BuildUpgradeOptions(Pawn pawn)
        {
            List<RobotUpgradeOption> options = new List<RobotUpgradeOption>();

            options.AddRange(SlotOptions(pawn, "Left Arm", BodyPartGroupDefOf_MrHandy.MrHandyArmLeft, Props.toolSwapCost,
                HediffDefOf_MrHandy.FCP_Hediff_MrHandy_Left_Saw, HediffDefOf_MrHandy.FCP_Hediff_MrHandy_Left_Pincer));
            options.AddRange(SlotOptions(pawn, "Center Arm", BodyPartGroupDefOf_MrHandy.MrHandyArmCenter, Props.toolSwapCost,
                HediffDefOf_MrHandy.FCP_Hediff_MrHandy_Center_Pincer));
            options.AddRange(CenterArmWeaponOptions(pawn));
            options.AddRange(SlotOptions(pawn, "Right Arm", BodyPartGroupDefOf_MrHandy.MrHandyArmRight, Props.toolSwapCost,
                HediffDefOf_MrHandy.FCP_Hediff_MrHandy_Right_Flamer, HediffDefOf_MrHandy.FCP_Hediff_MrHandy_Right_Pincer));

            options.AddRange(SlotOptions(pawn, "Left Eye", BodyPartGroupDefOf_MrHandy.MrHandyEyeLeft, Props.armorEyesCost,
                HediffDefOf_MrHandy.FCP_Hediff_MrHandy_Eye_Left, HediffDefOf_MrHandy.FCP_Hediff_MrHandy_Eye_Left_Armored));
            options.AddRange(SlotOptions(pawn, "Center Eye", BodyPartGroupDefOf_MrHandy.MrHandyEyeCenter, Props.armorEyesCost,
                HediffDefOf_MrHandy.FCP_Hediff_MrHandy_Eye_Center, HediffDefOf_MrHandy.FCP_Hediff_MrHandy_Eye_Center_Armored));
            options.AddRange(SlotOptions(pawn, "Right Eye", BodyPartGroupDefOf_MrHandy.MrHandyEyeRight, Props.armorEyesCost,
                HediffDefOf_MrHandy.FCP_Hediff_MrHandy_Eye_Right, HediffDefOf_MrHandy.FCP_Hediff_MrHandy_Eye_Right_Armored));

            if (!pawn.health.hediffSet.HasHediff(HediffDefOf_MrHandy.FCP_Hediff_MrHandy_BodyArmored))
            {
                options.Add(BuildUpgrade(pawn, "Armor", "FCP_MrHandyLoadout_InstallArmoredBody", Props.armorBodyCost, delegate
                {
                    ApplyArmorHediff(pawn, HediffDefOf_MrHandy.FCP_Hediff_MrHandy_BodyArmored);
                    TouchHediffGraphic(pawn, HediffDefOf_MrHandy.FCP_Hediff_MrHandy_BodyArmored);
                    pawn.Drawer.renderer.SetAllGraphicsDirty();
                }));
            }

            options.AddRange(RepairOptions(pawn));
            return options;
        }

        private IEnumerable<RobotUpgradeOption> SlotOptions(Pawn pawn, string slotLabel, BodyPartGroupDef group, List<ThingDefCountClass> cost, params HediffDef[] variants)
        {
            HediffDef current = CurrentVariant(pawn, group);
            foreach (HediffDef variant in variants)
            {
                if (variant == null || variant == current)
                {
                    continue;
                }
                HediffDef install = variant;
                yield return BuildUpgrade(pawn, slotLabel, () => "FCP_MrHandyLoadout_InstallSlot".Translate(slotLabel, install.LabelCap), cost,
                    () => SwapVariant(pawn, group, install));
            }
        }

        private IEnumerable<RobotUpgradeOption> CenterArmWeaponOptions(Pawn pawn)
        {
            HediffDef current = CurrentVariant(pawn, BodyPartGroupDefOf_MrHandy.MrHandyArmCenter);

            if (current != HediffDefOf_MrHandy.FCP_Hediff_MrHandy_Gun)
            {
                yield return WeaponOption(pawn, HediffDefOf_MrHandy.FCP_Hediff_MrHandy_Gun, Props.gunResearch, Props.gunCost);
            }
            if (current != HediffDefOf_MrHandy.FCP_Hediff_MrHandy_Laser)
            {
                yield return WeaponOption(pawn, HediffDefOf_MrHandy.FCP_Hediff_MrHandy_Laser, Props.laserResearch, Props.laserCost);
            }
            if (current != HediffDefOf_MrHandy.FCP_Hediff_MrHandy_Plasma)
            {
                yield return WeaponOption(pawn, HediffDefOf_MrHandy.FCP_Hediff_MrHandy_Plasma, Props.plasmaResearch, Props.plasmaCost);
            }
        }

        private RobotUpgradeOption WeaponOption(Pawn pawn, HediffDef hediffDef, ResearchProjectDef research, List<ThingDefCountClass> cost)
        {
            if (research != null && !research.IsFinished)
            {
                return new RobotUpgradeOption
                {
                    category = "Center Arm",
                    label = "FCP_MrHandyLoadout_InstallSlot".Translate("Center Arm", hediffDef.LabelCap),
                    disabledReason = "FCP_UpgradeRobot_NeedsResearch".Translate(research.LabelCap),
                };
            }
            return BuildUpgrade(pawn, "Center Arm", () => "FCP_MrHandyLoadout_InstallSlot".Translate("Center Arm", hediffDef.LabelCap), cost,
                () => SwapVariant(pawn, BodyPartGroupDefOf_MrHandy.MrHandyArmCenter, hediffDef));
        }

        private IEnumerable<RobotUpgradeOption> RepairOptions(Pawn pawn)
        {
            foreach (BodyPartGroupDef group in RepairableGroups)
            {
                BodyPartRecord part = FindPart(pawn, group);
                if (part == null || !pawn.health.hediffSet.PartIsMissing(part))
                {
                    continue;
                }

                BodyPartRecord partToRestore = part;
                yield return BuildUpgrade(pawn, "Repair", () => "FCP_MrHandyLoadout_Repair".Translate(partToRestore.Label), Props.repairPartCost, delegate
                {
                    pawn.health.RestorePart(partToRestore, null, true);
                    pawn.Drawer.renderer.SetAllGraphicsDirty();
                });
            }
        }

        private static void ApplyArmorHediff(Pawn pawn, HediffDef hediffDef)
        {
            if (pawn.health.hediffSet.HasHediff(hediffDef))
            {
                return;
            }
            pawn.health.AddHediff(hediffDef);
        }

        private RobotUpgradeOption BuildUpgrade(Pawn pawn, string category, string labelKey, List<ThingDefCountClass> cost, System.Action install)
        {
            return BuildUpgrade(pawn, category, () => labelKey.Translate(), cost, install);
        }

        private RobotUpgradeOption BuildUpgrade(Pawn pawn, string category, System.Func<string> label, List<ThingDefCountClass> cost, System.Action install)
        {
            bool afford = RobotUpgradeUtility.CanAffordCost(parent.Map, cost);
            return new RobotUpgradeOption
            {
                category = category,
                label = label(),
                cost = cost,
                disabledReason = afford ? null : "FCP_UpgradeRobot_MissingMaterials".Translate(),
                install = delegate
                {
                    if (RobotUpgradeUtility.TryConsumeCost(parent.Map, cost))
                    {
                        install();
                    }
                },
            };
        }
    }
}
