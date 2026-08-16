using System.Text;
using LudeonTK;
using UnityEngine;

namespace FCP.Core;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public static class DebugActionsPrefabRoof
{
    [DebugAction("FCP: Prefabs", "Create prefab (with roof)", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
    private static void CreatePrefabWithRoof()
    {
        DebugToolsGeneral.GenericRectTool("Create", rect =>
        {
            Map map = Find.CurrentMap;
            PrefabDef prefabDef = PrefabUtility.CreatePrefab(rect, DebugGenerationSettings.prefabCopyAllThings, DebugGenerationSettings.prefabCopyTerrain);
            List<PrefabRoofData> roofs = CaptureRoof(rect, map);

            StringBuilder sb = new StringBuilder();
            AppendPrefabXml(sb, prefabDef, rect, roofs);

            GUIUtility.systemCopyBuffer = sb.ToString();
            Messages.Message("Copied to clipboard", MessageTypeDefOf.NeutralEvent, historical: false);
        }, closeOnComplete: true);
    }

    private static List<PrefabRoofData> CaptureRoof(CellRect rect, Map map)
    {
        Dictionary<RoofDef, List<IntVec3>> byRoof = new Dictionary<RoofDef, List<IntVec3>>();
        foreach (IntVec3 cell in rect.Cells)
        {
            RoofDef roof = cell.GetRoof(map);
            if (roof == null)
                continue;

            if (!byRoof.TryGetValue(roof, out List<IntVec3> cells))
                byRoof[roof] = cells = new List<IntVec3>();
            cells.Add(cell);
        }

        List<PrefabRoofData> result = new List<PrefabRoofData>();
        foreach (KeyValuePair<RoofDef, List<IntVec3>> kvp in byRoof)
        {
            PrefabRoofData data = new PrefabRoofData { def = kvp.Key };
            foreach (CellRect covered in rect.EnumerateRectanglesCovering(c => kvp.Value.Contains(c)))
                data.rects.Add(covered.MovedBy(-rect.Min));
            result.Add(data);
        }
        return result;
    }

    private static void AppendPrefabXml(StringBuilder sb, PrefabDef prefabDef, CellRect rect, List<PrefabRoofData> roofs)
    {
        const string t = "  ";
        sb.AppendLine();
        sb.AppendLine("<PrefabDef>");
        sb.AppendLine(t + "<defName>NewPrefab</defName> <!-- rename -->");
        sb.AppendLine($"{t}<size>({rect.Size.x},{rect.Size.z})</size>");

        List<PrefabThingData> things = prefabDef.GetThings().Select(x => x.data).Distinct().ToList();
        if (things.Count > 0)
        {
            sb.AppendLine(t + "<things>");
            foreach (PrefabThingData thing in things)
                AppendThing(sb, thing, t);
            sb.AppendLine(t + "</things>");
        }

        List<PrefabTerrainData> terrain = prefabDef.GetTerrain().Select(x => x.data).Distinct().ToList();
        if (terrain.Count > 0)
        {
            sb.AppendLine(t + "<terrain>");
            foreach (PrefabTerrainData terrainData in terrain)
                AppendTerrain(sb, terrainData, t);
            sb.AppendLine(t + "</terrain>");
        }

        if (roofs.Count > 0)
        {
            sb.AppendLine(t + "<modExtensions>");
            sb.AppendLine(t + t + "<li Class=\"FCP.Core.PrefabRoofExtension\">");
            sb.AppendLine(t + t + t + "<roofs>");
            foreach (PrefabRoofData roof in roofs)
            {
                sb.AppendLine(t + t + t + t + "<li>");
                sb.AppendLine($"{t}{t}{t}{t}{t}<def>{roof.def.defName}</def>");
                sb.AppendLine(t + t + t + t + t + "<rects>");
                foreach (CellRect r in roof.rects)
                    sb.AppendLine($"{t}{t}{t}{t}{t}{t}<li>{r}</li>");
                sb.AppendLine(t + t + t + t + t + "</rects>");
                sb.AppendLine(t + t + t + t + "</li>");
            }
            sb.AppendLine(t + t + t + "</roofs>");
            sb.AppendLine(t + t + "</li>");
            sb.AppendLine(t + "</modExtensions>");
        }

        sb.AppendLine("</PrefabDef>");
    }

    private static void AppendThing(StringBuilder sb, PrefabThingData thing, string t)
    {
        sb.AppendLine(t + t + "<" + thing.def.defName + ">");
        if (thing.rects != null)
        {
            sb.AppendLine(t + t + t + "<rects>");
            foreach (CellRect r in thing.rects)
                sb.AppendLine($"{t}{t}{t}{t}<li>{r}</li>");
            sb.AppendLine(t + t + t + "</rects>");
        }
        else if (thing.positions != null)
        {
            sb.AppendLine(t + t + t + "<positions>");
            foreach (IntVec3 pos in thing.positions)
                sb.AppendLine($"{t}{t}{t}{t}<li>{pos}</li>");
            sb.AppendLine(t + t + t + "</positions>");
        }
        else
        {
            sb.AppendLine($"{t}{t}{t}<position>{thing.position}</position>");
        }

        if (thing.relativeRotation != RotationDirection.None)
            sb.AppendLine(t + t + t + "<relativeRotation>" + thing.relativeRotation + "</relativeRotation>");
        if (thing.stuff != null)
            sb.AppendLine(t + t + t + "<stuff>" + thing.stuff.defName + "</stuff>");
        if (thing.quality.HasValue)
            sb.AppendLine($"{t}{t}{t}<quality>{thing.quality}</quality>");
        if (thing.hp != 0)
            sb.AppendLine($"{t}{t}{t}<hp>{thing.hp}</hp>");
        if (thing.stackCountRange != IntRange.One)
            sb.AppendLine($"{t}{t}{t}<stackCountRange>{thing.stackCountRange.min}~{thing.stackCountRange.max}</stackCountRange>");
        if (thing.colorDef != null)
            sb.AppendLine($"{t}{t}{t}<colorDef>{thing.colorDef}</colorDef>");

        sb.AppendLine(t + t + "</" + thing.def.defName + ">");
    }

    private static void AppendTerrain(StringBuilder sb, PrefabTerrainData terrain, string t)
    {
        sb.AppendLine(t + t + "<" + terrain.def.defName + ">");
        if (terrain.color != null)
            sb.AppendLine($"{t}{t}{t}<color>{terrain.color}</color>");
        sb.AppendLine(t + t + t + "<rects>");
        foreach (CellRect r in terrain.rects)
            sb.AppendLine($"{t}{t}{t}{t}<li>{r}</li>");
        sb.AppendLine(t + t + t + "</rects>");
        sb.AppendLine(t + t + "</" + terrain.def.defName + ">");
    }
}
