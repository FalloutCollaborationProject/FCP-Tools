using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace FCP_Shuttles
{
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