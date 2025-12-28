using HarmonyLib;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using Verse;

namespace FCP.Core.TemperatureApparelPreference
{
    [StaticConstructorOnStartup]
    public static class Compatibility
    {
        private static readonly ConcurrentDictionary<ThingDef, object> cachedExtensions =
            new ConcurrentDictionary<ThingDef, object>();

        private static Type harmonyInitType;
        private static Type apparelExtensionType;

        private static MethodInfo getModExtensionClosed;
        private static FieldInfo shouldHideBodyField;
        private static FieldInfo shouldHideHeadField;

        static Compatibility()
        {
            try
            {
                harmonyInitType = AccessTools.TypeByName("FalloutCore.HarmonyInit");
                apparelExtensionType = AccessTools.TypeByName("FalloutCore.ApparelExtension");

                if (harmonyInitType == null || apparelExtensionType == null)
                    return;

                BindFalloutCoreHandles();
                if (getModExtensionClosed == null)
                    return;

                var harmony = new Harmony("FCP.Core.TemperatureApparelPreference.FalloutCoreCompat");
                PatchFalloutCore(harmony);

                Log.Message("[TemperatureApparelPreference] Applied FalloutCore ApparelExtension thread-safety compatibility patch.");
            }
            catch (Exception ex)
            {
                Log.Error("[TemperatureApparelPreference] FalloutCore compat init failed:\n" + ex);
            }
        }

        private static void BindFalloutCoreHandles()
        {
            // Def.GetModExtension<T>() (generic instance method, no parameters)
            MethodInfo openGeneric = null;
            var methods = typeof(Def).GetMethods(BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < methods.Length; i++)
            {
                var m = methods[i];
                if (m.Name != "GetModExtension") continue;
                if (!m.IsGenericMethodDefinition) continue;
                if (m.GetParameters().Length != 0) continue;

                openGeneric = m;
                break;
            }

            if (openGeneric == null)
                return;

            try
            {
                getModExtensionClosed = openGeneric.MakeGenericMethod(apparelExtensionType);
            }
            catch
            {
                getModExtensionClosed = null;
                return;
            }

            shouldHideBodyField = AccessTools.Field(apparelExtensionType, "shouldHideBody");
            shouldHideHeadField = AccessTools.Field(apparelExtensionType, "shouldHideHead");
        }

        private static void PatchFalloutCore(Harmony harmony)
        {
            MethodInfo shouldHideBody = AccessTools.Method(harmonyInitType, "ShouldHideBody", new Type[] { typeof(ThingDef) });
            MethodInfo shouldHideHead = AccessTools.Method(harmonyInitType, "ShouldHideHead", new Type[] { typeof(ThingDef) });

            if (shouldHideBody != null)
            {
                harmony.Patch(
                    shouldHideBody,
                    prefix: new HarmonyMethod(typeof(Compatibility), nameof(ShouldHideBody_Prefix)));
            }

            if (shouldHideHead != null)
            {
                harmony.Patch(
                    shouldHideHead,
                    prefix: new HarmonyMethod(typeof(Compatibility), nameof(ShouldHideHead_Prefix)));
            }
        }

        public static bool ShouldHideBody_Prefix(ThingDef def, ref bool __result)
        {
            __result = GetFalloutCoreFlag(def, shouldHideBodyField);
            return false;
        }

        public static bool ShouldHideHead_Prefix(ThingDef def, ref bool __result)
        {
            __result = GetFalloutCoreFlag(def, shouldHideHeadField);
            return false;
        }

        private static bool GetFalloutCoreFlag(ThingDef def, FieldInfo flagField)
        {
            if (def == null)
                return false;

            if (getModExtensionClosed == null || apparelExtensionType == null || flagField == null)
                return false;

            object ext = GetExtensionThreadSafe(def);
            if (ext == null)
                return false;

            try
            {
                object val = flagField.GetValue(ext);
                return val is bool b && b;
            }
            catch
            {
                return false;
            }
        }

        private static object GetExtensionThreadSafe(ThingDef def)
        {
            // GetOrAdd ensures only one value factory wins; safe under RimWorld's parallel render path.
            return cachedExtensions.GetOrAdd(def, d =>
            {
                try
                {
                    return getModExtensionClosed.Invoke(d, null);
                }
                catch
                {
                    return null;
                }
            });
        }
    }
}
