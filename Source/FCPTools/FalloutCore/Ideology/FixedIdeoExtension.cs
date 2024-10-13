using HarmonyLib;

// ReSharper disable UnassignedField.Global

namespace FCP.Ideology;

public class FixedIdeoExtension : DefModExtension
{
    public IdeoIconDef ideoIconDef;
    public IdeoColorDef ideoColorDef;
    public string memberName;
    public string adjective;
    public string ritualRoomName;
    public List<RoleOverride> roleOverrides;
    
    public void CopyToIdeo(Ideo ideo)
    {
        ideo.memberName = memberName ?? ideo.memberName;
        ideo.adjective = adjective ?? ideo.adjective;
        ideo.WorshipRoomLabel = ritualRoomName ?? ideo.WorshipRoomLabel;

        if (ideoIconDef != null)
        {
            LongEventHandler.ExecuteWhenFinished(delegate
            {
                ideo.SetIcon(ideoIconDef, ideoColorDef.colorDef ?? ideo.colorDef ?? IdeoFoundation.GetRandomColorDef(ideo));
            });
        }
    }
    
    public class RoleOverride
    {
        public PreceptDef preceptDef;
        public string newName;
        public bool disableApparelRequirements = false;
        public List<PreceptApparelRequirement> apparelRequirementsOverride;
    }
}

[HarmonyPatch]
public static class FixedIdeoExtensionPatches
{
    /// <summary>
    /// Copy the content of FixedIdeoExtension to a newly made fixed ideo
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(IdeoGenerator), "MakeFixedIdeo")]
    public static void IdeoGenerator_MakeFixedIdeo_Postfix(IdeoGenerationParms parms, Ideo __result)
    {
        var extension = parms.forFaction?.GetModExtension<FixedIdeoExtension>();
        extension?.CopyToIdeo(__result);
    }
    
    /// <summary>
    /// Patches RandomizeIcon so that it doesn't randomize if one already exists.
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(IdeoFoundation), "RandomizeIcon")]
    public static bool IdeoFoundation_RandomizeIcon_Prefix(IdeoFoundation __instance)
    {
        var ideo = __instance.ideo;
        return ideo.iconDef == null;
    }

    /// <summary>
    /// Runs Post InitPrecepts to do our role overrides, should also work as a point for rituals in the future.
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(IdeoFoundation), "InitPrecepts")]
    public static void IdeoFoundation_InitPrecepts_Postfix(IdeoGenerationParms parms, IdeoFoundation __instance)
    {
        var extension = parms.forFaction?.GetModExtension<FixedIdeoExtension>();
        if (extension == null)
            return;
        
        foreach (var precept in __instance.ideo.PreceptsListForReading)
        {
            if (precept is Precept_Role preceptRole)
            {
                var overrides = extension.roleOverrides.FirstOrDefault(x => x.preceptDef == preceptRole.def);
                if (overrides == null)
                    continue;

                if (overrides.newName != null)
                {
                    precept.SetName(overrides.newName);
                }

                if (overrides.disableApparelRequirements)
                {
                    precept.ApparelRequirements.Clear();
                }
                else if (overrides.apparelRequirementsOverride.Any())
                {
                    precept.ApparelRequirements = overrides.apparelRequirementsOverride;
                }
                continue;
            }
        }

    }

    
    /*
    /// <summary>
    /// Add forced standard precepts
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(IdeoFoundation), "AddSpecialPrecepts")]
    public static void IdeoFoundation_AddSpecialPrecepts_Prefix(IdeoGenerationParms parms, Ideo ___ideo, IdeoFoundation __instance)
    {
        var extension = parms.forFaction.GetModExtension<FixedIdeoExtension>();g
        
        if (!parms.fixedIdeo || extension == null)
            return;
        
        foreach (var preceptDef in extension.requiredPrecepts)
        {
            if (!__instance.CanAddForFaction(preceptDef, parms.forFaction, parms.disallowedPrecepts, true, false,
                    false, parms.classicExtra))
            {
                Log.Error($"Couldn't add {preceptDef.defName} to ideo {parms.name} for faction {parms.forFaction.defName}, maybe there is an imcompatible precept required by a meme?");
                continue;
            }
            ___ideo.AddPrecept(PreceptMaker.MakePrecept(preceptDef));
        }
    }*/
}