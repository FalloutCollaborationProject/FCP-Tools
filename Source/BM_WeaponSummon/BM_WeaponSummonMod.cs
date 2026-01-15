using HarmonyLib;
using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;

namespace BM_WeaponSummon
{
    public class BM_WeaponSummonMod : Mod
    {
        public BM_WeaponSummonMod(ModContentPack pack) : base(pack)
        {
			new Harmony("BM_WeaponSummonMod").PatchAll();
        }
    }




}
