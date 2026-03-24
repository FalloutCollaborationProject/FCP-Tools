namespace FCP.Core.Shuttles
{
	[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
	public class FactionModExtension : DefModExtension
	{
		public bool replaceShuttle;
		public TransportShipDef transportShipDef;
		public ThingDef customShuttle;
		public ThingDef customShuttleIncoming;
		public ThingDef customShuttleLeaving;
		public ThingDef customShuttleCrashing;
		public ThingDef customShuttleCrashed;
		public int maxPawnCountInOneShuttle;
		public int minDistanceBetweenShuttles = 20;
	}
}
