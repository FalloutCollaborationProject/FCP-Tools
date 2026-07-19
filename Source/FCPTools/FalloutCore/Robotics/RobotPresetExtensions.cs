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

    public class ProtectronPresetExtension : DefModExtension
    {
        public bool hasHead = true;
        public HediffDef head;
        public HediffDef hand;
        public Color? color;
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
