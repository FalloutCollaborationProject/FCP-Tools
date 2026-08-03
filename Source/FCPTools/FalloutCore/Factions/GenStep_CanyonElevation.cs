using UnityEngine;
using Verse;

namespace FCP.Factions;

public class GenStep_CanyonElevation : GenStep
{
	public FloatRange floorElevationRange = new FloatRange(0.1f, 0.3f);

	public float wallElevation = 1.1f;

	public FloatRange corridorHalfWidthRange = new FloatRange(5f, 11f);

	public float wallBandWidth = 6f;

	public override int SeedPart => 195830441;

	public override void Generate(Map map, GenStepParams parms)
	{
		if (map.TileInfo.WaterCovered)
			return;

		bool alongX = map.Size.x >= map.Size.z;
		int length = alongX ? map.Size.x : map.Size.z;
		int span = alongX ? map.Size.z : map.Size.x;

		float[] centerline = BuildCenterline(length, span);
		float[] halfWidths = BuildHalfWidths(length);
		float floorTarget = floorElevationRange.RandomInRange;

		MapGenFloatGrid elevation = MapGenerator.Elevation;
		foreach (IntVec3 cell in map.AllCells)
		{
			int along = alongX ? cell.x : cell.z;
			int across = alongX ? cell.z : cell.x;

			float halfWidth = halfWidths[along];
			float dist = Mathf.Abs(across - centerline[along]);

			if (dist <= halfWidth)
			{
				float t = dist / halfWidth;
				elevation[cell] = Mathf.Min(elevation[cell], Mathf.Lerp(floorTarget, floorTarget + 0.25f, t));
			}
			else if (dist <= halfWidth + wallBandWidth)
			{
				float t = (dist - halfWidth) / wallBandWidth;
				elevation[cell] = Mathf.Lerp(floorTarget + 0.25f, Mathf.Max(elevation[cell], wallElevation), t);
			}
		}
	}

	private float[] BuildCenterline(int length, int span)
	{
		float margin = span * 0.2f;
		float min = margin;
		float max = span - margin;

		float[] raw = new float[length];
		raw[0] = Rand.Range(min, max);

		float drift = 0f;
		for (int i = 1; i < length; i++)
		{
			drift = Mathf.Clamp(drift + Rand.Range(-1f, 1f), -3f, 3f);
			raw[i] = Mathf.Clamp(raw[i - 1] + drift * 0.15f, min, max);
		}

		float[] smoothed = new float[length];
		int window = Mathf.Max(3, length / 40);
		for (int i = 0; i < length; i++)
		{
			float sum = 0f;
			int count = 0;
			for (int j = Mathf.Max(0, i - window); j <= Mathf.Min(length - 1, i + window); j++)
			{
				sum += raw[j];
				count++;
			}
			smoothed[i] = sum / count;
		}
		return smoothed;
	}

	private float[] BuildHalfWidths(int length)
	{
		float baseWidth = corridorHalfWidthRange.RandomInRange;
		float freq = Rand.Range(0.02f, 0.05f);
		float phase = Rand.Range(0f, 1000f);

		float[] result = new float[length];
		for (int i = 0; i < length; i++)
		{
			float wobble = Mathf.PerlinNoise(i * freq, phase) - 0.5f;
			result[i] = Mathf.Max(3f, baseWidth + wobble * baseWidth);
		}
		return result;
	}
}
