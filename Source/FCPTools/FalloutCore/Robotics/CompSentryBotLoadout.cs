using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace FCP.Core.Robotics
{
    public enum SentryBotWeapon : byte
    {
        Minigun,
        Rocket
    }

    public class CompProperties_SentryBotLoadout : CompProperties
    {
        public List<ThingDefCountClass> minigunSwapCost = new List<ThingDefCountClass>();
        public List<ThingDefCountClass> rocketSwapCost = new List<ThingDefCountClass>();

        public CompProperties_SentryBotLoadout()
        {
            compClass = typeof(CompSentryBotLoadout);
        }
    }

    public class CompSentryBotLoadout : ThingComp
    {
        private SentryBotWeapon weapon = SentryBotWeapon.Minigun;
        private bool didInitialSetup;

        public CompProperties_SentryBotLoadout Props => (CompProperties_SentryBotLoadout)props;
        public SentryBotWeapon Weapon => weapon;

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

            weapon = pawn.kindDef == PawnKindDefOf_SentryBot.FCP_Pawnkind_SentryBot_Rocket
                ? SentryBotWeapon.Rocket
                : SentryBotWeapon.Minigun;

            EquipWeapon(pawn, WeaponHediffFor(weapon));

            pawn.SetColor(RobotUtility.GetBodyColor(pawn));
            RobotUtility.TouchBodyColor(pawn);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref weapon, "sentryBotWeapon", SentryBotWeapon.Minigun);
            Scribe_Values.Look(ref didInitialSetup, "sentryBotLoadoutInitialSetupDone");
        }

        private static HediffDef WeaponHediffFor(SentryBotWeapon w)
        {
            return w == SentryBotWeapon.Minigun
                ? HediffDefOf_SentryBot.FCP_Hediff_SentryBot_Minigun
                : HediffDefOf_SentryBot.FCP_Hediff_SentryBot_Rocket;
        }

        private static void EquipWeapon(Pawn pawn, HediffDef weaponDef)
        {
            if (pawn?.health == null || weaponDef == null)
            {
                return;
            }

            Hediff existing = pawn.health.hediffSet.hediffs.FirstOrDefault(h =>
                h.def == HediffDefOf_SentryBot.FCP_Hediff_SentryBot_Minigun ||
                h.def == HediffDefOf_SentryBot.FCP_Hediff_SentryBot_Rocket);
            if (existing != null)
            {
                if (existing.def == weaponDef)
                {
                    return;
                }
                pawn.health.RemoveHediff(existing);
            }
            pawn.health.AddHediff(weaponDef);
            pawn.Drawer.renderer.SetAllGraphicsDirty();
        }

        private void SwitchWeapon(SentryBotWeapon newWeapon)
        {
            Pawn pawn = Pawn;
            if (pawn?.health == null || newWeapon == weapon)
            {
                return;
            }

            weapon = newWeapon;
            EquipWeapon(pawn, WeaponHediffFor(weapon));
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

            if (weapon != SentryBotWeapon.Minigun)
            {
                List<ThingDefCountClass> cost = Props.minigunSwapCost;
                bool afford = RobotUpgradeUtility.CanAffordCost(parent.Map, cost);
                options.Add(new RobotUpgradeOption
                {
                    category = "Weapon",
                    label = "FCP_SentryBotLoadout_InstallMinigun".Translate(),
                    cost = cost,
                    disabledReason = afford ? null : "FCP_UpgradeRobot_MissingMaterials".Translate(),
                    install = delegate
                    {
                        if (RobotUpgradeUtility.TryConsumeCost(parent.Map, cost))
                        {
                            SwitchWeapon(SentryBotWeapon.Minigun);
                        }
                    },
                });
            }

            if (weapon != SentryBotWeapon.Rocket)
            {
                List<ThingDefCountClass> cost = Props.rocketSwapCost;
                bool afford = RobotUpgradeUtility.CanAffordCost(parent.Map, cost);
                options.Add(new RobotUpgradeOption
                {
                    category = "Weapon",
                    label = "FCP_SentryBotLoadout_InstallRocket".Translate(),
                    cost = cost,
                    disabledReason = afford ? null : "FCP_UpgradeRobot_MissingMaterials".Translate(),
                    install = delegate
                    {
                        if (RobotUpgradeUtility.TryConsumeCost(parent.Map, cost))
                        {
                            SwitchWeapon(SentryBotWeapon.Rocket);
                        }
                    },
                });
            }

            return options;
        }
    }
}
