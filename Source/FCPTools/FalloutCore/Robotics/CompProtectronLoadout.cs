using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace FCP.Core.Robotics
{
    public class CompProperties_ProtectronLoadout : CompProperties
    {
        public List<ThingDefCountClass> headSwapCost = new List<ThingDefCountClass>();
        public List<ThingDefCountClass> handSwapCost = new List<ThingDefCountClass>();
        public ResearchProjectDef gunHandResearch;
        public List<ThingDefCountClass> gunHandCost = new List<ThingDefCountClass>();

        public CompProperties_ProtectronLoadout()
        {
            compClass = typeof(CompProtectronLoadout);
        }
    }

    public class CompProtectronLoadout : ThingComp
    {
        private static readonly Color DefaultColor = new Color32(200, 200, 200, 255);

        private bool didInitialSetup;

        public CompProperties_ProtectronLoadout Props => (CompProperties_ProtectronLoadout)props;

        private Pawn Pawn => parent as Pawn;

        public static bool HasHead(Pawn pawn, HediffDef headDef) => HasHediffOnGroup(pawn, BodyPartGroupDefOf_Protectron.ProtectronHead, headDef);
        public static bool HasHand(Pawn pawn, HediffDef handDef) => HasHediffOnGroup(pawn, BodyPartGroupDefOf_Protectron.ProtectronHands, handDef);

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
            Scribe_Values.Look(ref didInitialSetup, "protectronLoadoutInitialSetupDone");
        }

        private void DoInitialSetup()
        {
            Pawn pawn = Pawn;
            if (pawn?.health == null)
            {
                return;
            }

            ProtectronPresetExtension preset = pawn.kindDef.GetModExtension<ProtectronPresetExtension>();
            bool wearHead = preset?.hasHead ?? true;
            HediffDef headDef = preset?.head ?? HediffDefOf_Protectron.FCP_Hediff_Protectron_Head_Default;
            HediffDef handDef = preset?.hand ?? HediffDefOf_Protectron.FCP_Hediff_Protectron_Hand_Default;

            if (wearHead)
            {
                SwapOnGroup(pawn, BodyPartGroupDefOf_Protectron.ProtectronHead, headDef);
            }
            SwapOnGroup(pawn, BodyPartGroupDefOf_Protectron.ProtectronHands, handDef);

            pawn.SetColor(preset?.color ?? DefaultColor);
            RobotUtility.TouchBodyColor(pawn);
            TouchHediffGraphic(pawn, headDef);
            TouchHediffGraphic(pawn, handDef);
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
            options.AddRange(HeadOptions(pawn));
            options.AddRange(HandOptions(pawn));
            return options;
        }

        private IEnumerable<RobotUpgradeOption> HeadOptions(Pawn pawn)
        {
            if (!HasHead(pawn, HediffDefOf_Protectron.FCP_Hediff_Protectron_Head_Default))
            {
                yield return InstallOption(pawn, "Head", "FCP_ProtectronLoadout_InstallDefaultHead", BodyPartGroupDefOf_Protectron.ProtectronHead, HediffDefOf_Protectron.FCP_Hediff_Protectron_Head_Default, Props.headSwapCost);
            }
            if (!HasHead(pawn, HediffDefOf_Protectron.FCP_Hediff_Protectron_Head_Construct))
            {
                yield return InstallOption(pawn, "Head", "FCP_ProtectronLoadout_InstallConstructHead", BodyPartGroupDefOf_Protectron.ProtectronHead, HediffDefOf_Protectron.FCP_Hediff_Protectron_Head_Construct, Props.headSwapCost);
            }
        }

        private IEnumerable<RobotUpgradeOption> HandOptions(Pawn pawn)
        {
            if (!HasHand(pawn, HediffDefOf_Protectron.FCP_Hediff_Protectron_Hand_Default))
            {
                yield return InstallOption(pawn, "Hands", "FCP_ProtectronLoadout_InstallDefaultHand", BodyPartGroupDefOf_Protectron.ProtectronHands, HediffDefOf_Protectron.FCP_Hediff_Protectron_Hand_Default, Props.handSwapCost);
            }
            if (!HasHand(pawn, HediffDefOf_Protectron.FCP_Hediff_Protectron_Hand_Work))
            {
                yield return InstallOption(pawn, "Hands", "FCP_ProtectronLoadout_InstallWorkHand", BodyPartGroupDefOf_Protectron.ProtectronHands, HediffDefOf_Protectron.FCP_Hediff_Protectron_Hand_Work, Props.handSwapCost);
            }
            if (!HasHand(pawn, HediffDefOf_Protectron.FCP_Hediff_Protectron_Hand_Gun))
            {
                if (Props.gunHandResearch != null && !Props.gunHandResearch.IsFinished)
                {
                    yield return new RobotUpgradeOption
                    {
                        category = "Hands",
                        label = "FCP_ProtectronLoadout_InstallGunHand".Translate(),
                        disabledReason = "FCP_UpgradeRobot_NeedsResearch".Translate(Props.gunHandResearch.LabelCap),
                    };
                }
                else
                {
                    yield return InstallOption(pawn, "Hands", "FCP_ProtectronLoadout_InstallGunHand", BodyPartGroupDefOf_Protectron.ProtectronHands, HediffDefOf_Protectron.FCP_Hediff_Protectron_Hand_Gun, Props.gunHandCost);
                }
            }
        }

        private RobotUpgradeOption InstallOption(Pawn pawn, string category, string labelKey, BodyPartGroupDef group, HediffDef hediffDef, List<ThingDefCountClass> cost)
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
                        SwapOnGroup(pawn, group, hediffDef);
                        TouchHediffGraphic(pawn, hediffDef);
                        pawn.Drawer.renderer.SetAllGraphicsDirty();
                    }
                },
            };
        }
    }
}
