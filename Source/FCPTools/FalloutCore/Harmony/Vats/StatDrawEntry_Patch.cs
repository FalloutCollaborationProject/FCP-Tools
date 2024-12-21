using System.Reflection;
using FCP.Core.Stats;
using HarmonyLib;

namespace FCP.Core.Vats;

[HarmonyPatch(typeof(StatDrawEntry))]
public static class StatDrawEntry_Patch
{
    [HarmonyPatch(nameof(StatDrawEntry.GetExplanationText))]
    [HarmonyPrefix]
    public static bool GetExplanationText_Patch(StatDrawEntry __instance, StatRequest optionalReq, ref string __result)
    {
        if (__instance is LegendaryStatDrawEntry lsde)
        {
            Type StatDrawEntryType = typeof(StatDrawEntry).GetNestedType("StatDrawEntry", BindingFlags.NonPublic);
            FieldInfo expTextField = StatDrawEntryType.GetField("explanationText", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo numberSenseField = StatDrawEntryType.GetField("numberSense", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo valueField = StatDrawEntryType.GetField("value", BindingFlags.Instance | BindingFlags.NonPublic);
            MethodInfo writeExpMeth = StatDrawEntryType.GetMethod("WriteExplanationTextInt", BindingFlags.Instance | BindingFlags.NonPublic);

            if (expTextField.GetValue(__instance) == null)
                writeExpMeth.Invoke(__instance, null);

            var expText = (string)expTextField.GetValue(__instance);
            __result =
                optionalReq.Empty || __instance.stat == null
                    ? expText
                    : string.Format(
                        "{0}\n\n{1}",
                        expText,
                        __instance.stat.Worker.GetExplanationFull(optionalReq, (ToStringNumberSense)numberSenseField.GetValue(__instance), (float)valueField.GetValue(__instance))
                    );
            return false;
        }

        return true;
    }
}
