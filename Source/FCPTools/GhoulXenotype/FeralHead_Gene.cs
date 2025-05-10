using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;
namespace FCP_Ghoul
{
    public class FeralHead_Gene : Gene
    {
        public float r;
        public float g;
        public float b;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref r, "r");
            Scribe_Values.Look(ref g, "g");
            Scribe_Values.Look(ref b, "b");
        }
    }
}
