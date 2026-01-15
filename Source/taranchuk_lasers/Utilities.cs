using System.Linq;
using Verse;

namespace taranchuk_lasers
{
    [HotSwappable]
    [StaticConstructorOnStartup]

    public static class Utilities
    {
        internal static readonly bool VehiclesEnabled = ModsConfig.ActiveModsInLoadOrder.Any(m => m.PackageId == "SmashPhil.VehicleFramework");
        internal static readonly bool FlightCombatEnabled = ModsConfig.ActiveModsInLoadOrder.Any(m => m.PackageId == "ShunTheWitch.CVNmechanoidwarfarecore");
        
        public static float GetBodyAngle(Thing thing)
        {
            if (VehiclesEnabled && FlightCombatEnabled && CVNWarfareUtilities.GetAngle(thing, out var anglecvn))
                return anglecvn;
            if (VehiclesEnabled && VehiclesUtilities.GetAngle(thing, out var angle))
                    return angle;
            return thing.Rotation.AsAngle;
        }

        
        private static class VehiclesUtilities
        {
            public static bool GetAngle(Thing thing, out float angle)
            {
                if (thing is Vehicles.VehiclePawn vehiclePawn)
                {
                    angle = vehiclePawn.Angle;
                    return true;
                }
                angle = 0;
                return false;
            }
            
        }

        private static class CVNWarfareUtilities
        {

            public static bool GetAngle(Thing thing, out float angle)
            {
                if (thing is Vehicles.VehiclePawn vehiclePawn)
                {
                    var comp = vehiclePawn.GetComp<taranchuk_flightcombat.CompFlightMode>();
                    if (comp != null && taranchuk_flightcombat.VehiclePawn_UpdateRotation_Patch.InFlightModeOrNonStandardAngle(vehiclePawn, comp))
                    {
                        angle = comp.CurAngle;
                        return true;
                    }
                }

                angle = 0;
                return false;
            }
        }
        
    }
}
