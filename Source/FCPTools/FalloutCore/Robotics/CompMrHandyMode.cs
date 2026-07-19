using RimWorld;
using Verse;

namespace FCP.Core.Robotics
{
    public enum MrHandyMode : byte
    {
        Wander,
        Guard,
        GuardPawn,
        Cook,
        Clean,
        Garden,
        BasicCare,
        Tend,
        Childcare
    }

    public class CompProperties_MrHandyMode : CompProperties
    {
        public CompProperties_MrHandyMode()
        {
            compClass = typeof(CompMrHandyMode);
        }
    }

    public class CompMrHandyMode : ThingComp
    {
        private MrHandyMode mode = MrHandyMode.Wander;
        private Pawn guardedPawn;

        public CompProperties_MrHandyMode Props => (CompProperties_MrHandyMode)props;
        public MrHandyMode Mode => mode;
        public Pawn GuardedPawn => guardedPawn;

        public void SetMode(MrHandyMode newMode)
        {
            mode = newMode;
            if (newMode != MrHandyMode.GuardPawn)
            {
                guardedPawn = null;
            }
        }

        public void SetGuardPawn(Pawn pawn)
        {
            mode = MrHandyMode.GuardPawn;
            guardedPawn = pawn;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref mode, "mrHandyMode", MrHandyMode.Wander);
            Scribe_References.Look(ref guardedPawn, "mrHandyGuardedPawn");
        }

        public override string CompInspectStringExtra()
        {
            if ((parent as Pawn)?.Faction != Faction.OfPlayer)
            {
                return "FCP_ProtectronMode_Inspect".Translate("FCP_RobotMode_Wandering".Translate());
            }

            if (mode == MrHandyMode.GuardPawn)
            {
                return "FCP_ProtectronMode_Inspect".Translate("FCP_SecuritronMode_GuardPawn".Translate(guardedPawn?.LabelShort ?? "?"));
            }

            return "FCP_ProtectronMode_Inspect".Translate(mode.ToString());
        }
    }
}
