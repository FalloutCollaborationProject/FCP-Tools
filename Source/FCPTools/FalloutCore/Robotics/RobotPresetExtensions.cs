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

    // A single named head+hand+color combo. Head and hand always travel together as one unit
    // so a wild roll can never mix e.g. a construction head with a police claw.
    public class ProtectronLoadoutPreset
    {
        public string id;
        public bool hasHead = true;
        public HediffDef head;
        public HediffDef hand;
        public Color color = new Color32(200, 200, 200, 255);

        // >0 makes this preset eligible for the random roll wild-spawned protectrons get.
        // Presets reserved for builds/quests/characters (picked explicitly by id) leave this at 0.
        public float wildSpawnWeight;
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
