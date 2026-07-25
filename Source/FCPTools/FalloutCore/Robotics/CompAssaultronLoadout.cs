using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace FCP.Core.Robotics
{
    public class CompProperties_AssaultronLoadout : CompProperties
    {
        public List<ThingDefCountClass> handSwapCost = new List<ThingDefCountClass>();
        public List<ThingDefCountClass> bodyTypeSwapCost = new List<ThingDefCountClass>();

        public CompProperties_AssaultronLoadout()
        {
            compClass = typeof(CompAssaultronLoadout);
        }
    }

    public class CompAssaultronLoadout : ThingComp
    {
        private static readonly Color DefaultColor = new Color32(210, 210, 215, 255);
        private static readonly Color ArmyColor = new Color32(75, 83, 51, 255);

        private bool didInitialSetup;

        public CompProperties_AssaultronLoadout Props => (CompProperties_AssaultronLoadout)props;

        private Pawn Pawn => parent as Pawn;

        public static bool HasHand(Pawn pawn, HediffDef handDef) => HasHediffOnGroup(pawn, BodyPartGroupDefOf_Assaultron.AssaultronHands, handDef);

        public static bool IsArmy(Pawn pawn)
        {
            return pawn?.health?.hediffSet?.hediffs.Any(h => h.def == HediffDefOf_Assaultron.FCP_Hediff_Assaultron_BodyType_Army) ?? false;
        }

        private static bool HasHediffOnGroup(Pawn pawn, BodyPartGroupDef group, HediffDef hediffDef)
        {
            if (pawn?.health?.hediffSet == null)
            {
                return false;
            }
            BodyPartRecord part = FindParts(pawn, group).FirstOrDefault();
            return part != null && !pawn.health.hediffSet.PartIsMissing(part)
                && pawn.health.hediffSet.hediffs.Any(h => h.def == hediffDef && h.Part == part);
        }

        private static IEnumerable<BodyPartRecord> FindParts(Pawn pawn, BodyPartGroupDef group)
        {
            return pawn.RaceProps.body.AllParts.Where(p => p.groups.Contains(group));
        }

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
            Scribe_Values.Look(ref didInitialSetup, "assaultronLoadoutInitialSetupDone");
        }

        private void DoInitialSetup()
        {
            Pawn pawn = Pawn;
            if (pawn?.health == null)
            {
                return;
            }

            SwapOnGroup(pawn, BodyPartGroupDefOf_Assaultron.AssaultronHands, HediffDefOf_Assaultron.FCP_Hediff_Assaultron_Hand_Claw);
            pawn.SetColor(RobotPaintOptions.General.RandomElement().color);
            RobotUtility.TouchBodyColor(pawn);
            TouchHediffGraphic(pawn, HediffDefOf_Assaultron.FCP_Hediff_Assaultron_Hand_Claw);
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

        private static void SwapOnGroup(Pawn pawn, BodyPartGroupDef group, HediffDef newHediff)
        {
            if (newHediff == null)
            {
                return;
            }

            foreach (BodyPartRecord part in FindParts(pawn, group))
            {
                Hediff existing = pawn.health.hediffSet.hediffs.FirstOrDefault(h => h.Part == part && h.def.GetModExtension<RobotHediffGraphic>() != null);
                if (existing != null)
                {
                    if (existing.def == newHediff)
                    {
                        continue;
                    }
                    pawn.health.RemoveHediff(existing);
                }
                pawn.health.AddHediff(newHediff, part);
            }
        }

        private static void SetArmyBodyType(Pawn pawn, bool army)
        {
            Hediff existing = pawn.health.hediffSet.hediffs.FirstOrDefault(h => h.def == HediffDefOf_Assaultron.FCP_Hediff_Assaultron_BodyType_Army);
            if (army)
            {
                if (existing == null)
                {
                    pawn.health.AddHediff(HediffDefOf_Assaultron.FCP_Hediff_Assaultron_BodyType_Army);
                }
                pawn.SetColor(ArmyColor);
            }
            else
            {
                if (existing != null)
                {
                    pawn.health.RemoveHediff(existing);
                }
                pawn.SetColor(DefaultColor);
            }
            RobotUtility.TouchBodyColor(pawn);
            pawn.Drawer.renderer.SetAllGraphicsDirty();
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
            options.AddRange(HandOptions(pawn));
            options.AddRange(BodyTypeOptions(pawn));
            return options;
        }

        private IEnumerable<RobotUpgradeOption> HandOptions(Pawn pawn)
        {
            if (!HasHand(pawn, HediffDefOf_Assaultron.FCP_Hediff_Assaultron_Hand_Claw))
            {
                yield return InstallOption(pawn, "Hands", "FCP_AssaultronLoadout_InstallClaw", HediffDefOf_Assaultron.FCP_Hediff_Assaultron_Hand_Claw, Props.handSwapCost);
            }
            if (!HasHand(pawn, HediffDefOf_Assaultron.FCP_Hediff_Assaultron_Hand_Blade))
            {
                yield return InstallOption(pawn, "Hands", "FCP_AssaultronLoadout_InstallBlade", HediffDefOf_Assaultron.FCP_Hediff_Assaultron_Hand_Blade, Props.handSwapCost);
            }
        }

        private RobotUpgradeOption InstallOption(Pawn pawn, string category, string labelKey, HediffDef hediffDef, List<ThingDefCountClass> cost)
        {
            bool afford = RobotUpgradeUtility.CanAffordCost(parent.Map, cost);
            return new RobotUpgradeOption
            {
                category = category,
                label = labelKey.Translate(),
                cost = cost,
                disabledReason = afford ? null : "FCP_UpgradeRobot_MissingMaterials".Translate(),
                install = delegate
                {
                    if (RobotUpgradeUtility.TryConsumeCost(parent.Map, cost))
                    {
                        SwapOnGroup(pawn, BodyPartGroupDefOf_Assaultron.AssaultronHands, hediffDef);
                        TouchHediffGraphic(pawn, hediffDef);
                        pawn.Drawer.renderer.SetAllGraphicsDirty();
                    }
                },
            };
        }

        private IEnumerable<RobotUpgradeOption> BodyTypeOptions(Pawn pawn)
        {
            bool army = IsArmy(pawn);
            if (army)
            {
                yield return InstallBodyTypeOption(pawn, "FCP_AssaultronLoadout_InstallStandardPlating", false);
            }
            else
            {
                yield return InstallBodyTypeOption(pawn, "FCP_AssaultronLoadout_InstallArmyPlating", true);
            }
        }

        private RobotUpgradeOption InstallBodyTypeOption(Pawn pawn, string labelKey, bool army)
        {
            List<ThingDefCountClass> cost = Props.bodyTypeSwapCost;
            bool afford = RobotUpgradeUtility.CanAffordCost(parent.Map, cost);
            return new RobotUpgradeOption
            {
                category = "Body",
                label = labelKey.Translate(),
                cost = cost,
                disabledReason = afford ? null : "FCP_UpgradeRobot_MissingMaterials".Translate(),
                install = delegate
                {
                    if (RobotUpgradeUtility.TryConsumeCost(parent.Map, cost))
                    {
                        SetArmyBodyType(pawn, army);
                    }
                },
            };
        }
    }
}
