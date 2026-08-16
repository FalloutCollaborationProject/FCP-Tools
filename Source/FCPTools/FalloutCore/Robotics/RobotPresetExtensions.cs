using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace FCP.Core.Robotics
{
    public class SecuritronPresetExtension : DefModExtension
    {
        public HediffDef face;
        public SecuritronWeapon weapon = SecuritronWeapon.Gun;
        public float rocketsChance;
    }

    public class ProtectronLoadoutPreset
    {
        public string id;
        public bool hasHead = true;
        public HediffDef head;
        public HediffDef hand;
        public Color color = new Color32(200, 200, 200, 255);

        public List<Color> colorOptions = new List<Color>();

        public float wildSpawnWeight;

        public Color RollColor() => colorOptions.Count > 0 ? colorOptions.RandomElement() : color;
    }

    public class ProtectronPresetExtension : DefModExtension
    {
        public List<ProtectronLoadoutPreset> presets = new List<ProtectronLoadoutPreset>();

        public ProtectronLoadoutPreset GetPreset(string id)
        {
            return presets.FirstOrDefault(p => p.id == id);
        }

        public ProtectronLoadoutPreset RandomWildPreset()
        {
            List<ProtectronLoadoutPreset> candidates = presets.Where(p => p.wildSpawnWeight > 0f).ToList();
            return candidates.Count > 0 ? candidates.RandomElementByWeight(p => p.wildSpawnWeight) : null;
        }
    }

    public enum MrHandyRole
    {
        Handy,
        Nanny,
        Gutsy,
        Orderly
    }

    public class MrHandyPresetExtension : DefModExtension
    {
        public MrHandyRole role;
        public HediffDef leftTool;
        public HediffDef centerTool;
        public HediffDef rightTool;
        public Color? color;
    }
}
