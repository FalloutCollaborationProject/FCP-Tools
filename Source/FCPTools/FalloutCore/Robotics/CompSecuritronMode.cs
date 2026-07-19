using Verse;

namespace FCP.Core.Robotics
{
    public enum SecuritronMode : byte
    {
        GuardHome,
        GuardPawn,
        GuardPoint
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
        private IntVec3 guardedPoint = IntVec3.Invalid;

        public CompProperties_SecuritronMode Props => (CompProperties_SecuritronMode)props;
        public SecuritronMode Mode => mode;
        public Pawn GuardedPawn => guardedPawn;
        public IntVec3 GuardedPoint => guardedPoint;

        public void SetGuardHome()
        {
            mode = SecuritronMode.GuardHome;
            guardedPawn = null;
            guardedPoint = IntVec3.Invalid;
        }

        public void SetGuardPawn(Pawn pawn)
        {
            mode = SecuritronMode.GuardPawn;
            guardedPawn = pawn;
            guardedPoint = IntVec3.Invalid;
        }

        public void SetGuardPoint(IntVec3 point)
        {
            mode = SecuritronMode.GuardPoint;
            guardedPawn = null;
            guardedPoint = point;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref mode, "securitronMode", SecuritronMode.GuardHome);
            Scribe_References.Look(ref guardedPawn, "guardedPawn");
            Scribe_Values.Look(ref guardedPoint, "guardedPoint", IntVec3.Invalid);
        }

        public override string CompInspectStringExtra()
        {
            if ((parent as Pawn)?.Faction != RimWorld.Faction.OfPlayer)
            {
                return "FCP_SecuritronMode_Inspect".Translate("FCP_RobotMode_Wandering".Translate());
            }

            return mode switch
            {
                SecuritronMode.GuardPawn => "FCP_SecuritronMode_Inspect".Translate("FCP_SecuritronMode_GuardPawn".Translate(guardedPawn?.LabelShort ?? "?")),
                SecuritronMode.GuardPoint => "FCP_SecuritronMode_Inspect".Translate("FCP_SecuritronMode_GuardPoint".Translate(guardedPoint.ToString())),
                _ => "FCP_SecuritronMode_Inspect".Translate("FCP_SecuritronMode_GuardHome".Translate()),
            };
        }
    }
}
