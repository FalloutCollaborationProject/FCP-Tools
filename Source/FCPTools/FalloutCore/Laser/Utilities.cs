using System.Linq;
using Verse;

namespace FCP.Core.Laser
{
    [HotSwappable]
    [StaticConstructorOnStartup]
    public static class Utilities
    {
        public static float GetBodyAngle(Thing thing)
        {
            return thing.Rotation.AsAngle;
        }
    }
}
