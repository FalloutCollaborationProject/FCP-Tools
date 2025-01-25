// Decompiled with JetBrains decompiler
// Type: FalloutCore.HarmonyInit
// Assembly: ApparelExtension, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 5CC3DCC6-E3C4-4626-942D-94AD2809ABD4
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\RimWorld\Mods\FCP-Tools\1.5\Assemblies\ApparelExtension.dll

using HarmonyLib;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace FCP.Core
{
    [StaticConstructorOnStartup]
    public static class ApparelExtensionProps
    {
        private static Dictionary<ThingDef, ApparelExtension> cachedExtensions = new Dictionary<ThingDef, ApparelExtension>();

        //static HarmonyInit() => new Harmony("FalloutCore.ApparelExtension").PatchAll();

        public static bool ShouldHideBody(this ThingDef def)
        {
            ApparelExtension modExtension;
            if (!cachedExtensions.TryGetValue(def, out modExtension))
                cachedExtensions[def] = modExtension = def.GetModExtension<ApparelExtension>();
            return modExtension != null && modExtension.shouldHideBody;
        }

        public static bool ShouldHideHead(this ThingDef def)
        {
            ApparelExtension modExtension;
            if (!cachedExtensions.TryGetValue(def, out modExtension))
                cachedExtensions[def] = modExtension = def.GetModExtension<ApparelExtension>();
            return modExtension != null && modExtension.shouldHideHead;
        }
    }
}
