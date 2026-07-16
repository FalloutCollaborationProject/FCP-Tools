using UnityEngine;
using Verse;

namespace FCP.Core.Robotics
{
    public class SecuritronPresetExtension : DefModExtension
    {
        public ThingDef face;
        public SecuritronWeapon weapon = SecuritronWeapon.Gun;
        public float rocketsChance;
    }

    public class ProtectronPresetExtension : DefModExtension
    {
        public bool hasHead = true;
        public ThingDef head;
        public ThingDef hand;
        public Color? color;
    }
}
