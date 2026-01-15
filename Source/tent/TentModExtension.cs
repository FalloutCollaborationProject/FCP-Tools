using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Tent
{
    public class TentModExtension : DefModExtension
    {
        public bool negateWater = false;
        public bool negateSleptOutside = false;
        public bool negateSleptInCold = false;
        public bool negateSleptInHeat = false;
        public bool negateSleptInBarracks = false;
        public bool ideologyTentAssignmentAllowed = false;
        public HediffDef customHediff = null;
    }
}