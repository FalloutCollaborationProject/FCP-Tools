using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace FCP.Enlist;

class EnlistMod : Mod
{
    public static EnlistSettings settings;
    public EnlistMod(ModContentPack pack) : base(pack)
    {
        settings = GetSettings<EnlistSettings>();
    }

    public override void WriteSettings()
    {
        base.WriteSettings();
        EnlistUtils.DoDefsRemoval();
    }
}