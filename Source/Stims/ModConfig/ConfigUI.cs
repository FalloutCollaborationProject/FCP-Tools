using UnityEngine;
using Verse;

namespace StimPacks.ModConfig
{
    public class ConfigUI : Mod
    {
            public static Configs Config;
            public ConfigUI(ModContentPack content) : base(content)
            {
                Config = GetSettings<Configs>();
            }
    }
}