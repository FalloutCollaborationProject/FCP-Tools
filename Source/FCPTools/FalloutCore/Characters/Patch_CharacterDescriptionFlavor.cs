using HarmonyLib;
using Verse;

namespace FCP.Core
{
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.DescriptionFlavor), MethodType.Getter)]
    public static class Patch_CharacterDescriptionFlavor
    {
        public static void Postfix(Pawn __instance, ref string __result)
        {
            UniqueCharactersTracker tracker = UniqueCharactersTracker.Instance;
            if (tracker != null
                && tracker.TryGetPawnCharacter(__instance, out UniqueCharacter character)
                && !character.def.description.NullOrEmpty())
            {
                __result = character.def.description;
            }
        }
    }
}
