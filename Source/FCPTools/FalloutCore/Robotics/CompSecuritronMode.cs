using Verse;

namespace FCP.Core.Robotics
{
    public enum SecuritronMode : byte
    {
        GuardHome,
        GuardPawn
    }

    public class CompProperties_SecuritronMode : CompProperties
    {
        public CompProperties_SecuritronMode()
        {
            compClass = typeof(CompSecuritronMode);
        }
    }

    public class CompSecuritronMode : ThingComp
    {
        private SecuritronMode mode = SecuritronMode.GuardHome;
        private Pawn guardedPawn;

        public CompProperties_SecuritronMode Props => (CompProperties_SecuritronMode)props;
        public SecuritronMode Mode => mode;
        public Pawn GuardedPawn => guardedPawn;

        public void SetGuardHome()
        {
            mode = SecuritronMode.GuardHome;
            guardedPawn = null;
        }

        public void SetGuardPawn(Pawn pawn)
        {
            mode = SecuritronMode.GuardPawn;
            guardedPawn = pawn;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref mode, "securitronMode", SecuritronMode.GuardHome);
            Scribe_References.Look(ref guardedPawn, "guardedPawn");
        }

        public override string CompInspectStringExtra()
        {
            if ((parent as Pawn)?.Faction != RimWorld.Faction.OfPlayer)
            {
                return "FCP_SecuritronMode_Inspect".Translate("FCP_RobotMode_Wandering".Translate());
            }

            return mode == SecuritronMode.GuardHome
                ? "FCP_SecuritronMode_Inspect".Translate("FCP_SecuritronMode_GuardHome".Translate())
                : "FCP_SecuritronMode_Inspect".Translate("FCP_SecuritronMode_GuardPawn".Translate(guardedPawn?.LabelShort ?? "?"));
        }
    }
}
