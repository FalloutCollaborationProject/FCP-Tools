using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
using UnityEngine;

namespace FCP.PocketMaps
{
    public class CompProperties_PocketMapControl : CompProperties
    {
        public CompProperties_PocketMapControl()
        {
            compClass = typeof(CompPocketMapControl);
        }
    }

    public class CompPocketMapControl : ThingComp
    {
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var gizmo in base.CompGetGizmosExtra())
                yield return gizmo;

            var map = parent.Map;
            if (map?.Parent == null) yield break;

            var portal = FindPortalForMap(map);
            if (portal == null) yield break;

            if (portal is PresettableMapPortal presettablePortal && presettablePortal.IsAbandoned)
                yield break;

            if (map.info.parent.Faction == null)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Claim pocket map",
                    defaultDesc = "Claim this pocket map for your colony.",
                    icon = ContentFinder<Texture2D>.Get("UI/Designators/Claim"),
                    action = () =>
                    {
                        map.info.parent.SetFaction(Faction.OfPlayer);
                        Messages.Message("Pocket map claimed.", MessageTypeDefOf.PositiveEvent);
                    }
                };
            }

            yield return new Command_Action
            {
                defaultLabel = "Abandon pocket map",
                defaultDesc = "Abandon this pocket map permanently. All colonists will be evacuated and the map will be destroyed.",
                icon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel"),
                action = () =>
                {
                    Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                        "Abandon this pocket map? All colonists will be evacuated and you cannot re-enter.",
                        () => Abandon(map, portal),
                        destructive: true
                    ));
                }
            };
        }

        private MapPortal FindPortalForMap(Map targetMap)
        {
            foreach (var map in Find.Maps)
            {
                if (map == targetMap) continue;
                foreach (var thing in map.listerThings.AllThings.OfType<MapPortal>())
                {
                    if (thing.GetOtherMap() == targetMap)
                        return thing;
                }
            }
            return null;
        }

        private void Abandon(Map map, MapPortal portal)
        {
            var entrancePortal = portal;
            
            if (portal is PocketMapExit)
            {
                entrancePortal = FindPortalForMap(map);
            }
            
            if (entrancePortal == null)
            {
                Log.Error("Could not find entrance portal to mark as abandoned");
                return;
            }

            // Mark portal as abandoned using direct field
            if (entrancePortal is PresettableMapPortal presettablePortal)
            {
                presettablePortal.MarkAsAbandoned();
            }

            var colonists = map.mapPawns.AllPawnsSpawned.Where(p => p.IsColonist).ToList();
            var parentMap = entrancePortal.Map;
            var exitPos = entrancePortal.Position;

            foreach (var pawn in colonists)
            {
                var pos = CellFinder.RandomClosewalkCellNear(exitPos, parentMap, 3);
                pawn.DeSpawn();
                GenSpawn.Spawn(pawn, pos, parentMap);
                pawn.jobs.StopAll();
            }

            if (map.info.parent != null && Find.WorldObjects.Contains(map.info.parent))
                Find.WorldObjects.Remove(map.info.parent);
            
            Current.Game.DeinitAndRemoveMap(map, false);
            
            Messages.Message($"{colonists.Count} colonist(s) evacuated. Pocket map abandoned.", MessageTypeDefOf.NegativeEvent);
        }
    }
}
