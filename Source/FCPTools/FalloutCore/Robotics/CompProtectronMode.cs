using Verse;

namespace FCP.Core.Robotics
{
    public enum ProtectronMode : byte
    {
        Guard,
        Construct,
        Haul
    }

    public class CompProperties_ProtectronMode : CompProperties
    {
        public CompProperties_ProtectronMode()
        {
            compClass = typeof(CompProtectronMode);
        }
    }

    public class CompProtectronMode : ThingComp
    {
        private ProtectronMode mode = ProtectronMode.Haul;

        public CompProperties_ProtectronMode Props => (CompProperties_ProtectronMode)props;
        public ProtectronMode Mode => mode;

        public void SetMode(ProtectronMode newMode)
        {
            mode = newMode;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref mode, "protectronMode", ProtectronMode.Haul);
        }

        public override string CompInspectStringExtra()
        {
            if ((parent as Pawn)?.Faction != RimWorld.Faction.OfPlayer)
            {
                return "FCP_ProtectronMode_Inspect".Translate("FCP_RobotMode_Wandering".Translate());
            }

            return "FCP_ProtectronMode_Inspect".Translate(mode.ToString());
        }
    }
}
