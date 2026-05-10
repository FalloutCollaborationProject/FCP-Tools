namespace FCP.Core;

public static class GauntletUtility
{
    public static List<Pawn> GetNearbyPawnInLineOfSight(this IntVec3 center, Map map, float radius, bool needLoS)
    {
        IReadOnlyList<Pawn> list = map.mapPawns.AllPawnsSpawned;
        List<Pawn> result = new List<Pawn>();
        float squaredDistance = radius * radius;
        for (int i = list.Count - 1; i >= 0; i--)
        {
            Pawn pawn = list[i];
            if (pawn.Dead) continue;
            if (needLoS && !GenSight.LineOfSightToThing(center, pawn, map)) continue;
            float distance = squaredDistance + 1f;
            if (pawn.Spawned)
            {
                distance = pawn.Position.DistanceToSquared(center);
            }
            else if (pawn.Corpse != null)
            {
                distance = pawn.Corpse.Position.DistanceToSquared(center);
            }

            if (distance > squaredDistance)
            {
                continue;
            }
            result.Add(pawn);
        }
        return result;
    }
    
    public static IEnumerable<Pawn> NearbyPawnInLineOfSight(this IntVec3 center, Map map, float radius, bool needLoS)
    {
        IReadOnlyList<Pawn> list = map.mapPawns.AllPawnsSpawned;
        float squaredDistance = radius * radius;
        for (int i = list.Count - 1; i >= 0; i--)
        {
            Pawn pawn = list[i];
            if (pawn.Dead) continue;
            if (needLoS && !GenSight.LineOfSightToThing(center, pawn, map)) continue;
            float distance = squaredDistance + 1f;
            if (pawn.Spawned)
            {
                distance = pawn.Position.DistanceToSquared(center);
            }
            else if (pawn.Corpse != null)
            {
                distance = pawn.Corpse.Position.DistanceToSquared(center);
            }

            if (distance > squaredDistance)
            {
                continue;
            }
            yield return pawn;
        }
    }
}
