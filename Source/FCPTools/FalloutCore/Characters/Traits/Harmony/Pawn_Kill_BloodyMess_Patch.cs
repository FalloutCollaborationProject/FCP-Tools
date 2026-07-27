using HarmonyLib;
using RimWorld;
using Verse;
using Verse.Sound;

namespace FCP.Core.Traits;

[HarmonyPatch(typeof(Pawn), nameof(Pawn.Kill))]
public static class Pawn_Kill_BloodyMess_Patch
{
    private static SoundDef fleshBurstSound;
    private static SoundDef FleshBurstSound => fleshBurstSound ??= DefDatabase<SoundDef>.GetNamed("BulletImpact_Flesh");

    public static void Postfix(Pawn __instance, DamageInfo? dinfo)
    {
        Map map = __instance.Corpse?.Map ?? __instance.MapHeld;
        if (map == null)
            return;

        Pawn instigator = dinfo?.Instigator as Pawn;
        if (instigator?.story?.traits == null || !instigator.story.traits.HasTrait(TraitsDefOf.FCP_Trait_Bloody_Mess))
            return;

        IntVec3 position = __instance.Corpse?.Position ?? __instance.PositionHeld;

        GibCorpse(__instance);

        foreach (IntVec3 cell in GenRadial.RadialCellsAround(position, 1.5f, true))
        {
            if (cell.InBounds(map) && Rand.Chance(0.6f))
                FilthMaker.TryMakeFilth(cell, map, ThingDefOf.Filth_Blood, Rand.RangeInclusive(1, 2));
        }

        SoundInfo soundInfo = SoundInfo.InMap(new TargetInfo(position, map));
        SoundStarter.PlayOneShot(FleshBurstSound, soundInfo);
    }

    private static void GibCorpse(Pawn pawn)
    {
        BodyPartRecord corePart = pawn.RaceProps?.body?.corePart;
        if (corePart == null)
            return;

        foreach (BodyPartRecord part in pawn.RaceProps.body.AllParts)
        {
            if (part == corePart || pawn.health.hediffSet.HasMissingPartFor(part))
                continue;
            pawn.health.AddHediff(HediffDefOf.MissingBodyPart, part, null, null);
        }

        pawn.health.DropBloodFilth();
        pawn.Drawer?.renderer?.SetAllGraphicsDirty();
    }
}
