using Verse;

namespace FCP.Core.Robotics
{
    public enum EyebotMode : byte
    {
        Defend,
        Scavenge,
        Explore,
        Music
    }

    public class CompProperties_EyebotMode : CompProperties
    {
        public CompProperties_EyebotMode()
        {
            compClass = typeof(CompEyebotMode);
        }
    }

    public class CompEyebotMode : ThingComp
    {
        private EyebotMode mode = EyebotMode.Defend;

        public CompProperties_EyebotMode Props => (CompProperties_EyebotMode)props;
        public EyebotMode Mode => mode;

        public void SetMode(EyebotMode newMode)
        {
            mode = newMode;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref mode, "eyebotMode", EyebotMode.Defend);
        }

        public override string CompInspectStringExtra()
        {
            if ((parent as Pawn)?.Faction != RimWorld.Faction.OfPlayer)
            {
                return "FCP_EyebotMode_Inspect".Translate("FCP_RobotMode_Wandering".Translate());
            }

            return "FCP_EyebotMode_Inspect".Translate(mode.ToString());
        }
    }
}
