using System.Linq;
using Verse;

namespace FCP.Core.Laser
{
    [HotSwappable]
    [StaticConstructorOnStartup]
    public static class Utilities
    {
        // Optional mod support removed for compilation - vehicle angle support requires those mods' assemblies
        // Core functionality works fine without them - vehicle support can be added later if needed
        
        public static float GetBodyAngle(Thing thing)
        {
            // For now, just use the standard rotation angle
            // Vehicle support would require Vehicles Framework and CVN MechanoidWarfare assemblies
            return thing.Rotation.AsAngle;
        }
    }
}
