using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace BiomesKit;

public sealed class HarmonyStarter : Mod
{
	public const string HarmonyId = "net.biomes.terrainkit";

	public HarmonyStarter(ModContentPack content)
		: base(content)
	{
		Assembly executingAssembly = Assembly.GetExecutingAssembly();
		string name = executingAssembly.GetName().Name;
		Version version = executingAssembly.GetName().Version;
		Version version2 = version;
		foreach (ModContentPack item in LoadedModManager.RunningModsListForReading)
		{
			foreach (FileInfo item2 in ModContentPack.GetAllFilesForMod(item, "Assemblies/", (string e) => e.ToLower() == ".dll").Select(delegate(KeyValuePair<string, FileInfo> f)
			{
				KeyValuePair<string, FileInfo> keyValuePair = f;
				return keyValuePair.Value;
			}))
			{
				AssemblyName assemblyName = AssemblyName.GetAssemblyName(item2.FullName);
				if (assemblyName.Name == name && assemblyName.Version > version2)
				{
					version2 = assemblyName.Version;
					Log.Error(string.Format("<color=cyan>Incorrect load order:</color> BiomesKit load order error detected. {0} is loading an older version {1} before {2} loads version {3}. Please put {2} above {0} in the mod list.", content.Name, version, item.Name, version2));
				}
			}
		}
		new Harmony("net.biomes.terrainkit").PatchAll(executingAssembly);
	}
}
