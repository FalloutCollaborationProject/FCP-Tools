using Verse;

namespace FCP.Core.Robotics
{
    public class CompProperties_MusicHolotape : CompProperties
    {
        public SoundDef trackToPlay;

        public CompProperties_MusicHolotape()
        {
            compClass = typeof(CompMusicHolotape);
        }
    }

    public class CompMusicHolotape : ThingComp
    {
        public CompProperties_MusicHolotape Props => (CompProperties_MusicHolotape)props;
    }
}
