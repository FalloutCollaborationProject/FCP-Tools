using Verse;

namespace FCP.Core.Robotics
{
    public enum AssaultronMode : byte
    {
        GuardHome,
        GuardPawn,
        Assault
    }

    public class CompProperties_AssaultronMode : CompProperties
    {
        public CompProperties_AssaultronMode()
        {
            compClass = typeof(CompAssaultronMode);
        }
    }

    public class CompAssaultronMode : ThingComp
    {
        private AssaultronMode mode = AssaultronMode.GuardHome;
        private Pawn guardedPawn;

        public CompProperties_AssaultronMode Props => (CompProperties_AssaultronMode)props;
        public AssaultronMode Mode => mode;
        public Pawn GuardedPawn => guardedPawn;

        public void SetGuardHome()
        {
            mode = AssaultronMode.GuardHome;
            guardedPawn = null;
        }

        public void SetGuardPawn(Pawn pawn)
        {
            mode = AssaultronMode.GuardPawn;
            guardedPawn = pawn;
        }

        public void SetAssault()
        {
            mode = AssaultronMode.Assault;
            guardedPawn = null;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref mode, "assaultronMode", AssaultronMode.GuardHome);
            Scribe_References.Look(ref guardedPawn, "guardedPawn");
        }

        public override string CompInspectStringExtra()
        {
            if ((parent as Pawn)?.Faction != RimWorld.Faction.OfPlayer)
            {
                return "FCP_AssaultronMode_Inspect".Translate("FCP_RobotMode_Wandering".Translate());
            }

            return mode switch
            {
                AssaultronMode.GuardPawn => "FCP_AssaultronMode_Inspect".Translate("FCP_SecuritronMode_GuardPawn".Translate(guardedPawn?.LabelShort ?? "?")),
                AssaultronMode.Assault => "FCP_AssaultronMode_Inspect".Translate("FCP_AssaultronMode_Assault".Translate()),
                _ => "FCP_AssaultronMode_Inspect".Translate("FCP_SecuritronMode_GuardHome".Translate()),
            };
        }
    }
}
