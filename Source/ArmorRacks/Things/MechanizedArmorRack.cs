using ArmorRacks.Drawers;
using UnityEngine;
using Verse;

namespace ArmorRacks.Things
{
    public class MechanizedArmorRack: ArmorRack
    {
        public MechanizedArmorRack(): base()
        {
            ContentsDrawer = new MechanizedArmorRackContentsDrawer(this);
        }
    }
}