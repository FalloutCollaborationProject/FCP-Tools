using System.Collections.Generic;
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

        public static bool HasHead(Pawn pawn, ThingDef headDef) => HasApparel(pawn, headDef);
        public static bool HasHand(Pawn pawn, ThingDef handDef) => HasApparel(pawn, handDef);

        private static bool HasApparel(Pawn pawn, ThingDef apparelDef)
        {
            if (pawn?.apparel == null)
            {
                return false;
            }
            foreach (Apparel apparel in pawn.apparel.WornApparel)
            {
                if (apparel.def == apparelDef)
                {
                    return true;
                }
            }
            return false;
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
            if (pawn?.apparel == null)
            {
                return;
            }

            ProtectronPresetExtension preset = pawn.kindDef.GetModExtension<ProtectronPresetExtension>();
            bool wearHead = preset?.hasHead ?? true;
            ThingDef headDef = preset?.head ?? ThingDefOf_ProtectronLoadout.FCP_Apparel_Protectron_Head_Default;
            ThingDef handDef = preset?.hand ?? ThingDefOf_ProtectronLoadout.FCP_Apparel_Protectron_Hand_Default;

            if (wearHead && !HasApparel(pawn, headDef))
            {
                Wear(pawn, headDef);
            }
            if (!HasApparel(pawn, handDef))
            {
                Wear(pawn, handDef);
            }
            if (handDef == ThingDefOf_ProtectronLoadout.FCP_Apparel_Protectron_Hand_Gun)
            {
                EquipGun(pawn);
            }

            pawn.SetColor(preset?.color ?? DefaultColor);

            RobotUtility.TouchBodyColor(pawn);
            pawn.Drawer.renderer.SetAllGraphicsDirty();
            RetintApparel(pawn);
        }

        public static void RetintApparel(Pawn pawn)
        {
            if (pawn == null)
            {
                return;
            }

            Color color = RobotUtility.GetBodyColor(pawn);

            if (pawn.equipment?.Primary != null)
            {
                CompColorableUtility.SetColor(pawn.equipment.Primary, color, reportFailure: false);
                RobotUtility.TouchGraphic(pawn.equipment.Primary);
            }

            if (pawn.apparel == null)
            {
                return;
            }

            foreach (Apparel apparel in pawn.apparel.WornApparel)
            {
                if (apparel.TryGetComp<CompColorable>() != null)
                {
                    CompColorableUtility.SetColor(apparel, color, reportFailure: false);
                    RobotUtility.TouchGraphic(apparel);
                }
            }

            pawn.Drawer.renderer.SetAllGraphicsDirty();
        }

        private static void Wear(Pawn pawn, ThingDef apparelDef)
        {
            Apparel newApparel = (Apparel)ThingMaker.MakeThing(apparelDef);
            pawn.apparel.Wear(newApparel, dropReplacedApparel: false);
            RobotUtility.TouchGraphic(newApparel);
        }

        private static void EquipGun(Pawn pawn)
        {
            if (pawn?.equipment == null)
            {
                return;
            }

            if (pawn.equipment.Primary != null)
            {
                pawn.equipment.DestroyEquipment(pawn.equipment.Primary);
            }

            ThingWithComps gun = (ThingWithComps)ThingMaker.MakeThing(ThingDefOf_ProtectronLoadout.FCP_Gun_Protectron_Arm);
            pawn.equipment.AddEquipment(gun);
            RobotUtility.TouchGraphic(gun);
        }

        private static void UnequipGun(Pawn pawn)
        {
            if (pawn?.equipment?.Primary != null)
            {
                pawn.equipment.DestroyEquipment(pawn.equipment.Primary);
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

            foreach (FloatMenuOption option in HeadOptions(pawn))
            {
                yield return option;
            }
            foreach (FloatMenuOption option in HandOptions(pawn))
            {
                yield return option;
            }
        }

        private IEnumerable<FloatMenuOption> HeadOptions(Pawn pawn)
        {
            if (!HasHead(pawn, ThingDefOf_ProtectronLoadout.FCP_Apparel_Protectron_Head_Default))
            {
                yield return InstallOption(pawn, "FCP_ProtectronLoadout_InstallDefaultHead", ThingDefOf_ProtectronLoadout.FCP_Apparel_Protectron_Head_Default, Props.headSwapCost);
            }
            if (!HasHead(pawn, ThingDefOf_ProtectronLoadout.FCP_Apparel_Protectron_Head_Construct))
            {
                yield return InstallOption(pawn, "FCP_ProtectronLoadout_InstallConstructHead", ThingDefOf_ProtectronLoadout.FCP_Apparel_Protectron_Head_Construct, Props.headSwapCost);
            }
        }

        private IEnumerable<FloatMenuOption> HandOptions(Pawn pawn)
        {
            if (!HasHand(pawn, ThingDefOf_ProtectronLoadout.FCP_Apparel_Protectron_Hand_Default))
            {
                yield return InstallHandOption(pawn, "FCP_ProtectronLoadout_InstallDefaultHand", ThingDefOf_ProtectronLoadout.FCP_Apparel_Protectron_Hand_Default);
            }
            if (!HasHand(pawn, ThingDefOf_ProtectronLoadout.FCP_Apparel_Protectron_Hand_Work))
            {
                yield return InstallHandOption(pawn, "FCP_ProtectronLoadout_InstallWorkHand", ThingDefOf_ProtectronLoadout.FCP_Apparel_Protectron_Hand_Work);
            }
            if (!HasHand(pawn, ThingDefOf_ProtectronLoadout.FCP_Apparel_Protectron_Hand_Gun))
            {
                yield return InstallHandOption(pawn, "FCP_ProtectronLoadout_InstallGunHand", ThingDefOf_ProtectronLoadout.FCP_Apparel_Protectron_Hand_Gun);
            }
        }

        private FloatMenuOption InstallOption(Pawn pawn, string labelKey, ThingDef apparelDef, List<ThingDefCountClass> cost)
        {
            if (!RobotUpgradeUtility.CanAffordCost(parent.Map, cost))
            {
                return new FloatMenuOption(labelKey.Translate() + ": " + "FCP_UpgradeRobot_MissingMaterials".Translate(), null);
            }

            return new FloatMenuOption(labelKey.Translate(), delegate
            {
                if (RobotUpgradeUtility.TryConsumeCost(parent.Map, cost))
                {
                    Wear(pawn, apparelDef);
                }
            });
        }

        private FloatMenuOption InstallHandOption(Pawn pawn, string labelKey, ThingDef handDef)
        {
            List<ThingDefCountClass> cost = Props.handSwapCost;
            if (!RobotUpgradeUtility.CanAffordCost(parent.Map, cost))
            {
                return new FloatMenuOption(labelKey.Translate() + ": " + "FCP_UpgradeRobot_MissingMaterials".Translate(), null);
            }

            return new FloatMenuOption(labelKey.Translate(), delegate
            {
                if (!RobotUpgradeUtility.TryConsumeCost(parent.Map, cost))
                {
                    return;
                }

                Wear(pawn, handDef);
                if (handDef == ThingDefOf_ProtectronLoadout.FCP_Apparel_Protectron_Hand_Gun)
                {
                    EquipGun(pawn);
                }
                else
                {
                    UnequipGun(pawn);
                }
                RetintApparel(pawn);
            });
        }
    }
}
