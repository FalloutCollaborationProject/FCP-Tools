using System.Reflection;
using System.Reflection.Emit;
using FCP.Factions;
using HarmonyLib;
using RimWorld.Planet;
using UnityEngine;
using Verse.AI;

namespace FCP.Core;

[StaticConstructorOnStartup]
public static class Patches
{
    static Patches()
    {
        var harmony = FCPCoreMod.harmony;
        // Biome Feature Requirements
        harmony.Patch(original: AccessTools.Method(typeof(WildAnimalSpawner), "CommonalityOfAnimalNow"),
            postfix: new HarmonyMethod(typeof(Patches), nameof(WildAnimalSpawnerCommonalityOfAnimalNow_Postfix)));
        
        // Non Slaves in Traders
        harmony.Patch(original: AccessTools.Method(typeof(TraderCaravanUtility), "GetTraderCaravanRole"),
            postfix: new HarmonyMethod(typeof(Patches), nameof(TraderCaravanUtilityGetTraderCaravanRole_Postfix)));
        
        harmony.Patch(original: AccessTools.Method(typeof(Pawn_GuestTracker), "RandomizeJoinStatus"),
            postfix: new HarmonyMethod(typeof(Patches), nameof(Pawn_GuestTrackerRandomizeJoinStatus_Postfix)));
        
        harmony.Patch(original: AccessTools.Method(typeof(Pawn), "PreTraded"),
            postfix: new HarmonyMethod(typeof(Patches), nameof(PawnPreTraded_Postfix)));
        
        // Flying Pawns
        harmony.Patch(original: AccessTools.Method(typeof(DamageWorker_AddInjury), "ApplyDamageToPart"),
            prefix: new HarmonyMethod(typeof(Patches), nameof(DamageWorker_AddInjuryApplyDamageToPart_Prefix)));
        
        harmony.Patch(original: AccessTools.Method(typeof(JobGiver_AIDefendPawn), "FindAttackTarget"),
            postfix: new HarmonyMethod(typeof(Patches), nameof(JobGiver_AIDefendPawnFindAttackTarget_Postfix)));
        
        harmony.Patch(original: AccessTools.Method(typeof(Pawn_JobTracker), "CleanupCurrentJob"),
            prefix: new HarmonyMethod(typeof(Patches), nameof(Pawn_JobTrackerCleanupCurrentJob_Prefix)));
        
        harmony.Patch(original: AccessTools.Method(typeof(Verb_MeleeAttack), "GetDodgeChance"),
            postfix: new HarmonyMethod(typeof(Patches), nameof(Verb_MeleeAttackGetDodgeChance_Postfix)));
        
        harmony.Patch(original: AccessTools.Method(typeof(Pawn_JobTracker), nameof(Pawn_JobTracker.StartJob)),
            postfix: new HarmonyMethod(typeof(Patches), nameof(Pawn_JobTrackerStartJob_Postfix)));
        
        harmony.Patch(original: AccessTools.PropertyGetter(typeof(ShotReport), nameof(ShotReport.AimOnTargetChance_StandardTarget)),
            postfix: new HarmonyMethod(typeof(Patches), nameof(ShotReportAimOnTargetChance_StandardTarget_Postfix)));
        
        harmony.Patch(original: AccessTools.Method(typeof(StatExtension), nameof(StatExtension.GetStatValue)),
            postfix: new HarmonyMethod(typeof(Patches), nameof(StatExtensionGetStatValue_Postfix)));
        
        // Faction Fixed Ideology
        harmony.Patch(original: AccessTools.Method(typeof(IdeoGenerator), "MakeFixedIdeo"),
            postfix: new HarmonyMethod(typeof(Patches), nameof(IdeoGeneratorMakeFixedIdeo_Postfix)));
        
        harmony.Patch(original: AccessTools.Method(typeof(IdeoFoundation), "RandomizeIcon"),
            prefix: new HarmonyMethod(typeof(Patches), nameof(IdeoFoundationRandomizeIcon_Prefix)));
        
        harmony.Patch(original: AccessTools.Method(typeof(IdeoFoundation), "InitPrecepts"),
            postfix: new HarmonyMethod(typeof(Patches), nameof(IdeoFoundationInitPrecepts_Postfix)));
        
        // Banned Arrival Modes
        harmony.Patch(original: AccessTools.Method(typeof(PawnsArrivalModeWorker), "CanUseWith"),
            postfix: new HarmonyMethod(typeof(Patches), nameof(PawnsArrivalModeWorkerCanUseWith_Postfix)));
        
        // Hidden Faction Traders
        harmony.Patch(
            original: typeof(IncidentWorker_CaravanMeeting).GetNestedTypes(AccessTools.all)
                .SelectMany(AccessTools.GetDeclaredMethods)
                .First(mi => mi.ReturnType == typeof(bool) && 
                             mi.GetParameters().ContainsAny(pi => pi.ParameterType == typeof(Faction))),
            transpiler: new HarmonyMethod(typeof(Patches), nameof(IncidentWorker_CaravanMeetingTryFindFaction_Linq_Transpiler)));
        
        harmony.Patch(original: AccessTools.Method(typeof(IncidentWorker_NeutralGroup), "FactionCanBeGroupSource"),
            transpiler: new HarmonyMethod(typeof(Patches), nameof(IncidentWorker_NeutralGroup_FactionCanBeGroupSource_Transpiler)));
        
        // Max Title for Permits
        harmony.Patch(original: AccessTools.Method(typeof(PermitsCardUtility), "DoLeftRect"),
            transpiler: new HarmonyMethod(typeof(Patches), nameof(PermitsCardUtility_LeftRect_Transpiler)));
        
        harmony.Patch(original: AccessTools.Method(typeof(RoyalTitlePermitDef), "AvailableForPawn"),
            postfix: new HarmonyMethod(typeof(Patches), nameof(RoyalTitlePermitDef_AvailableForPawn_Postfix)));
        
        harmony.Patch(original: AccessTools.Method(typeof(RoyalTitleAwardWorker), "DoAward"),
            postfix: new HarmonyMethod(typeof(Patches), nameof(RoyalTitleAwardWorker_DoAward_Postfix)));
        
        harmony.Patch(original: AccessTools.Method(typeof(RoyalTitleAwardWorker_Instant), "DoAward"),
            postfix: new HarmonyMethod(typeof(Patches), nameof(RoyalTitleAwardWorker_DoAward_Postfix)));
        
        // Forced TraderKindDef for PawnGroupMaker
        harmony.Patch(original: AccessTools.Method(typeof(PawnGroupKindWorker_Trader), "GeneratePawns", parameters: [typeof(PawnGroupMakerParms), typeof(PawnGroupMaker), typeof(List<Pawn>), typeof(bool)]),
            prefix: new HarmonyMethod(typeof(Patches), nameof(PawnGroupKindWorker_Trader_GeneratePawns_Prefix)));
        
        // Forced TraderKindDef for PawnGroupMaker
        harmony.Patch(original: AccessTools.Method(typeof(PawnGroupKindWorker_Trader), "GenerateTrader", parameters: [typeof(PawnGroupMakerParms), typeof(PawnGroupMaker), typeof(TraderKindDef)]),
            prefix: new HarmonyMethod(typeof(Patches), nameof(PawnGroupKindWorker_Trader_GenerateTrader_Prefix)));
        
        // Forced TraderKindDef for PawnGroupMaker
        harmony.Patch(original: AccessTools.Method(typeof(PawnGroupKindWorker_Normal), "GeneratePawns", parameters: [typeof(PawnGroupMakerParms), typeof(PawnGroupMaker), typeof(List<Pawn>), typeof(bool)]),
            postfix: new HarmonyMethod(typeof(Patches), nameof(PawnGroupKindWorker_Normal_GeneratePawns_Postfix)));
        
        // Faction Permanent Hostility
        harmony.Patch(original: AccessTools.Method(typeof(GoodwillSituationWorker_PermanentEnemy), "ArePermanentEnemies"),
            postfix: new HarmonyMethod(typeof(Patches), nameof(GoodwillSituationWorker_PermanentEnemy_ArePermanentEnemies_Postfix)));
        
        harmony.Patch(original: AccessTools.Method(typeof(Faction), "CanChangeGoodwillFor"),
            postfix: new HarmonyMethod(typeof(Patches), nameof(Faction_CanChangeGoodwillFor_Postfix)));
        
        harmony.Patch(original: AccessTools.Method(typeof(FactionDef), "PermanentlyHostileTo"),
            postfix: new HarmonyMethod(typeof(Patches), nameof(FactionDef_PermanentlyHostileTo_Postfix)));
        
        harmony.Patch(original: typeof(Faction).GetDeclaredMethods().First(mi => mi.Name.Contains("GetInitialGoodwill")),
            prefix: new HarmonyMethod(typeof(Patches), nameof(Faction_TryMakeInitialRelationsWith_GetInitialGoodwill_Prefix)));
        
        // Unique Characters
        harmony.Patch(original: AccessTools.Method(typeof(Faction), nameof(Faction.TryGenerateNewLeader)),
            prefix: new HarmonyMethod(typeof(Patches), nameof(Faction_TryGenerateNewLeader_Prefix)));
        
        // Weapon Sprite Adjustment
        harmony.Patch(original: AccessTools.Method(typeof(PawnRenderUtility), nameof(PawnRenderUtility.DrawEquipmentAndApparelExtras)),
            prefix: new HarmonyMethod(typeof(Patches), nameof(WeaponDrawPosPatch)));
        
        //Rarity Label
        harmony.Patch(original: AccessTools.Method(typeof(InspectPaneUtility), nameof(InspectPaneUtility.AdjustedLabelFor)),
            postfix: new HarmonyMethod(typeof(Patches), nameof(RarityLabelPatch)));
    }

    #region Biome Feature Requirements

    /// <summary>
    /// Ensures certain environmental conditions are met before a pawn can be (naturally) spawned on the map.
    /// </summary>
    public static void WildAnimalSpawnerCommonalityOfAnimalNow_Postfix(PawnKindDef def, ref Map ___map,
        ref float __result)
    {
        ModExtension_BiomeFeatureRequirements extension = def.race.GetModExtension<ModExtension_BiomeFeatureRequirements>();

        if (extension is null) return;
        bool hasNoCaves = extension.requireCaves && !Find.World.HasCaves(___map.Tile);
        bool hasNoCoast = extension.requireCoast && !Find.World.CoastDirectionAt(___map.Tile).IsValid;
        bool hasNoHills = extension.requireHills && ___map.Tile.Tile.hilliness == Hilliness.Flat;
        bool hasNoRiver = extension.requireRiver && (___map.Tile.Tile as SurfaceTile)?.Rivers == null;

        if (hasNoCaves || hasNoCoast || hasNoHills || hasNoRiver)
        {
            __result = 0;
        }
    }

    #endregion

    #region Non Slaves in Traders

    /// <summary>
    /// Makes it so that custom pawnKinds can be sold as 'slaves'.
    /// </summary>
    public static void TraderCaravanUtilityGetTraderCaravanRole_Postfix(Pawn p, ref TraderCaravanRole __result)
    {
        ModExtension_PawnKindProperties props = ModExtension_PawnKindProperties.Get(p.kindDef);
        if (props is { purchasableFromTrader: true })
        {
            __result = TraderCaravanRole.Chattel;
        }
    }

    /// <summary>
    /// Ensures recruit pawn kinds are not sold into slavery.
    /// </summary>
    public static void Pawn_GuestTrackerRandomizeJoinStatus_Postfix(ref Pawn ___pawn, ref JoinStatus ___joinStatus)
    {
        if (___joinStatus != JoinStatus.JoinAsColonist && PatchesUtility.CanRecruit(___pawn))
        {
            ___joinStatus = JoinStatus.JoinAsColonist;
        }
    }

    /// <summary>
    /// Remove the thought as recruits are not slaves.
    /// </summary>
    public static void PawnPreTraded_Postfix(ref Pawn __instance)
    {
        if (!PatchesUtility.CanRecruit(__instance)) return;
        Need_Mood moodNeed = __instance.needs.mood;
        moodNeed?.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.FreedFromSlavery);
    }

    #endregion

    #region Flying Pawns

    /// <summary>
    /// Modifies how damage is applied to a specific body part of a Pawn.
    /// When the instigator (the entity dealing the damage) is a flying pawn.
    /// </summary>
    public static void DamageWorker_AddInjuryApplyDamageToPart_Prefix(ref DamageInfo dinfo, Pawn pawn,
        DamageWorker.DamageResult result)
    {
        Thing instigator = dinfo.Instigator;

        if (instigator is not Pawn instigatorPawn ||
            !instigatorPawn.IsFlyingPawn(out CompFlyingPawn flyingComp)) return;
        AttackDamageFactor attackDamage = flyingComp.Props.attackDamageFactor;

        if (attackDamage == null) return;
        if (pawn.BodySize <= attackDamage.targetBodySize)
        {
            dinfo.SetAmount(dinfo.Amount * attackDamage.damageMultiplier);
        }
    }

    /// <summary>
    /// Ensures flying pawns will prioritize attacking the same enemies that their master is currently engaging with.
    /// </summary>
    public static void JobGiver_AIDefendPawnFindAttackTarget_Postfix(JobGiver_AIDefendPawn __instance,
        ref Thing __result, Pawn pawn)
    {
        if (pawn == null || __instance is not JobGiver_AIDefendMaster) return;
        if (!pawn.IsFlyingPawn(out CompFlyingPawn flyingComp)) return;
        if (!flyingComp.Props.attackEnemiesMasterAttacking) return;

        Pawn defendee = pawn.playerSettings.Master;
        if (defendee.CurJobDef == JobDefOf.AttackStatic || defendee.CurJobDef == JobDefOf.AttackMelee)
        {
            __result = defendee.CurJob.targetA.Thing;
        }
    }

    /// <summary>
    /// Ensures that whenever a flying pawn finishes its current job it will stop flying if it was flying during the job.
    /// Pawn’s appearance will be updated to reflect its grounded status.
    /// </summary>
    public static void Pawn_JobTrackerCleanupCurrentJob_Prefix(Pawn ___pawn)
    {
        if (!___pawn.IsFlyingPawn(out CompFlyingPawn comp)) return;
        if (!comp.isFlyingCurrently) return;
        comp.isFlyingCurrently = false;
        comp.ChangeGraphic();
    }

    /// <summary>
    /// Modifies the melee attack dodge chance for pawns that are currently flying.
    /// </summary>
    public static void Verb_MeleeAttackGetDodgeChance_Postfix(ref float __result, LocalTargetInfo target)
    {
        Pawn pawn = target.Pawn;

        if (!pawn.IsFlyingPawn(out CompFlyingPawn flyingComp) || !flyingComp.isFlyingCurrently) return;
        __result *= flyingComp.Props.evadeChanceWhenFlying;
    }

    /// <summary>
    /// Controls when a flying pawn should take flight based on the current job it is assigned.
    /// </summary>
    public static void Pawn_JobTrackerStartJob_Postfix(Pawn ___pawn)
    {
        if (!___pawn.IsFlyingPawn(out CompFlyingPawn comp)) return;
        Job curJob = ___pawn.CurJob;

        if (curJob == null) return;
        if ((!comp.Props.flyWhenFleeing || curJob.def != JobDefOf.Flee && curJob.def != JobDefOf.FleeAndCower)
            && (curJob.def != JobDefOf.GotoWander || !Rand.Chance(comp.Props.flyWhenWanderingChance))
            && (curJob.def != JobDefOf.PredatorHunt || !comp.Props.flyWhenHunting)) return;

        comp.isFlyingCurrently = true;
        curJob.locomotionUrgency = LocomotionUrgency.Jog;
        comp.ChangeGraphic(true);
    }

    /// <summary>
    /// Adjusts the hit chance when a flying pawn is targeted.
    /// Reduces the accuracy based on the pawn's flying status.
    /// </summary>
    public static void ShotReportAimOnTargetChance_StandardTarget_Postfix(ref float __result, TargetInfo ___target)
    {
        if (___target.Thing is not Pawn pawn) return;
        CompFlyingPawn compFlying = pawn.TryGetComp<CompFlyingPawn>();

        if (compFlying is { isFlyingCurrently: true })
        {
            __result *= 1f - compFlying.Props.evadeChanceWhenFlying;
        }
    }

    /// <summary>
    /// Adjusts the movement speed of flying pawns based on a multiplier when they are in flight.
    /// </summary>
    private static void StatExtensionGetStatValue_Postfix(Thing thing, StatDef stat, ref float __result)
    {
        if (stat == StatDefOf.MoveSpeed && thing is Pawn pawn &&
            pawn.IsFlyingPawn(out CompFlyingPawn comp) && comp.isFlyingCurrently)
        {
            __result *= comp.Props.flyingMoveSpeedMultiplier;
        }
    }

    #endregion

    #region Faction Fixed Ideology

    /// <summary>
    /// Copy the content of FixedIdeoExtension to a newly made fixed ideo.
    /// </summary>
    public static void IdeoGeneratorMakeFixedIdeo_Postfix(IdeoGenerationParms parms, Ideo __result)
    {
        ModExtension_FixedIdeo extension = parms.forFaction?.GetModExtension<ModExtension_FixedIdeo>();
        extension?.CopyToIdeo(__result);
    }

    /// <summary>
    /// Skips idea randomization if one already exists.
    /// </summary>
    public static bool IdeoFoundationRandomizeIcon_Prefix(IdeoFoundation __instance)
    {
        Ideo ideo = __instance.ideo;
        return ideo.iconDef == null;
    }

    /// <summary>
    /// Runs Post InitPrecepts to do our role overrides, should also work as a point for rituals in the future.
    /// </summary>
    public static void IdeoFoundationInitPrecepts_Postfix(IdeoGenerationParms parms, IdeoFoundation __instance)
    {
        ModExtension_FixedIdeo extension = parms.forFaction?.GetModExtension<ModExtension_FixedIdeo>();
        if (extension == null) return;

        foreach (Precept precept in __instance.ideo.PreceptsListForReading)
        {
            if (precept is not Precept_Role preceptRole) continue;

            ModExtension_FixedIdeo.RoleOverride overrides = extension.roleOverrides
                .FirstOrDefault(x => x.preceptDef == preceptRole.def);

            if (overrides == null) continue;
            if (overrides.newName != null)
            {
                preceptRole.SetName(overrides.newName);
            }

            if (overrides.disableApparelRequirements)
            {
                preceptRole.ApparelRequirements.Clear();
            }
            else if (overrides.apparelRequirementsOverride.Any())
            {
                preceptRole.ApparelRequirements = overrides.apparelRequirementsOverride;
            }
        }
    }

    #endregion

    #region Banned Arrival Modes

    /// <summary>
    /// Patch for the base PawnsArrivalModeWorker to disallow certain ArrivalModes based on the faction's extension
    /// </summary>
    public static void PawnsArrivalModeWorkerCanUseWith_Postfix(IncidentParms parms, ref bool __result,
        PawnsArrivalModeDef ___def)
    {
        if (__result == false)
            return;

        ModExtension_FactionBannedArrivalModes extension = parms.faction?.def.GetModExtension<ModExtension_FactionBannedArrivalModes>();
        if (extension != null && extension.arrivalModes.NotNullAndContains(___def))
        {
            __result = false;
        }
    }

    #endregion

    #region Hidden Faction Traders

    /// <summary>
    /// Patch for a compiler generated class for the linq code in the TryFindFaction Method,
    /// Allows hidden factions to be selected assuming they have the appropriate extension.
    /// </summary>
    private static IEnumerable<CodeInstruction> IncidentWorker_CaravanMeetingTryFindFaction_Linq_Transpiler(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator)
    {
        MethodInfo factionGetHiddenMethod = typeof(Faction).PropertyGetter(nameof(Faction.Hidden));

        CodeMatcher matcher = new CodeMatcher(instructions, generator)
            .End()
            .MatchStartBackwards( // Find the last return when the branch fails.
                new CodeMatch(OpCodes.Ldc_I4_0),
                new CodeMatch(OpCodes.Ret)
            )
            .MatchStartBackwards( // Find the use of get_Hidden so we can modify that branch
                CodeMatch.Calls(factionGetHiddenMethod),
                CodeMatch.Branches()
            )
            .Advance(1)
            .ThrowIfInvalid(
                "FCPTools Transpiler was unable to find the use of Faction.get_Hidden in the CaravanMeeting Nested Method");

        matcher.CreateLabelAt(matcher.Pos + 1, out Label nextConditional)
            .InsertAndAdvance(
                new CodeInstruction(OpCodes.Brfalse, nextConditional),
                CodeInstruction.LoadArgument(1), // Load the Faction field
                CodeInstruction.Call(typeof(ModExtension_HiddenFactionHasCaravans),
                    nameof(ModExtension_HiddenFactionHasCaravans.FactionHas))
            )
            .SetOpcodeAndAdvance(OpCodes.Brfalse);

        return matcher.Instructions();
    }

    /// <summary>
    /// Allow hidden factions with the ModExtension_HiddenFactionHasCaravans extension
    /// to be chosen for random trader groups
    /// </summary>
    private static IEnumerable<CodeInstruction> IncidentWorker_NeutralGroup_FactionCanBeGroupSource_Transpiler(
        IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        MethodInfo factionGetHiddenMethod = typeof(Faction).PropertyGetter(nameof(Faction.Hidden));

        CodeMatcher matcher = new CodeMatcher(instructions, generator)
            .MatchEndForward(
                CodeMatch.Calls(factionGetHiddenMethod),
                CodeMatch.Branches()
            )
            .ThrowIfInvalid("FCPTools : FactionCanBeGroupSource_Transpiler couldn't find a valid insertion point");

        object failurePoint = matcher.Operand;

        matcher.CreateLabelWithOffsets(1, out Label nextCheckPoint)
            .SetAndAdvance(OpCodes.Brfalse, nextCheckPoint)
            .Insert(
                CodeInstruction.LoadArgument(1), // Load Faction
                CodeInstruction.Call(typeof(ModExtension_HiddenFactionHasCaravans),
                    nameof(ModExtension_HiddenFactionHasCaravans.FactionHas)),
                new CodeInstruction(OpCodes.Brfalse, failurePoint)
            );

        return matcher.Instructions();
    }

    #endregion

    #region Max Title for Permits
        
    /// <summary>
    /// Lists the max title requirement of a given permitdef if one exists in the UI, using MaxTitlePermitExtension
    /// </summary>
    private static IEnumerable<CodeInstruction> PermitsCardUtility_LeftRect_Transpiler(
        IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        // Fields:
        // - PermitsCardUtility
        FieldInfo permitsCardUtilitySelectedPermitField = AccessTools.Field(typeof(PermitsCardUtility), "selectedPermit");
        // - RoyalTitlePermitDef
        FieldInfo permitDefMinTitleField = AccessTools.Field(typeof(RoyalTitlePermitDef), "minTitle");
        FieldInfo permitDefPrerequisiteField = AccessTools.Field(typeof(RoyalTitlePermitDef), "prerequisite");

        // Transpiler Procedure:
        // 1 - Find the minTitle check and the branching point after
        // 2 - Find the next branch which uses permitdef prerequisite and selectedPermit
        // 3 - Go back one to the end of the previous branch to check it.
        // 4 - Move back and load the required variables
        // 5 - Run the code and store the result

        // 1 - 3
        CodeMatcher matcher = new CodeMatcher(instructions, generator)
            // 1
            .MatchEndForward(
                CodeMatch.LoadsField(permitDefMinTitleField),
                CodeMatch.Branches()
            )
            .ThrowIfInvalid(
                "PermitsCardUtility_LeftRect_Transpiler: FCPTools couldn't find the correct branch (seq 1)")
            // 2
            .MatchStartForward(
                CodeMatch.LoadsField(permitsCardUtilitySelectedPermitField),
                CodeMatch.LoadsField(permitDefPrerequisiteField)
            )
            .ThrowIfInvalid(
                "PermitsCardUtility_LeftRect_Transpiler: FCPTools couldn't find the second branch (seq 2)")
            // 3
            .Advance(-1) // temporarily move back
            .ThrowIfNotMatch(
                "PermitsCardUtility_LeftRect_Transpiler: instruction prior to end of second branch is not as expected (seq 4)",
                CodeMatch.StoresLocal("storeText")
            );

        // Get the index for storing text
        int textIndex = (matcher.NamedMatch("storeText").operand as LocalBuilder)!.LocalIndex;

        // 4 - 5
        matcher.Advance(1) // move back up to the insertion point
            .Insert(
                // 4
                CodeInstruction.LoadLocal(textIndex), // text Field
                CodeInstruction.LoadArgument(1), // Pawn
                // 5
                CodeInstruction.Call(typeof(Patches), nameof(PermitsCardUtility_Util_AppendMaxTitleStatus)),
                CodeInstruction.StoreLocal(textIndex)
            );

        return matcher.Instructions();

    }

    // Retrieves the Extension and if necessary, append the max title text.
    private static string PermitsCardUtility_Util_AppendMaxTitleStatus(string text, Pawn pawn)
    {
        MaxTitlePermitExtension permitExtension = PermitsCardUtility.selectedPermit.GetModExtension<MaxTitlePermitExtension>();
        if (permitExtension?.maxTitle == null) return text;
            
        bool meetsMaxTitleRequirements = pawn.royalty.GetCurrentTitle(PermitsCardUtility.selectedFaction).seniority
                                         <= permitExtension.maxTitle.seniority;

        return text + "\n" + "Maximum Title: " + permitExtension.maxTitle.GetLabelForBothGenders()
            .Colorize(meetsMaxTitleRequirements ? Color.white : ColorLibrary.RedReadable);

    }
        
        
    /// <summary>
    /// Makes a PermitDef unavailable if the current title exceeds the def's MaxTitlePermitExtension max
    /// </summary>
    private static void RoyalTitlePermitDef_AvailableForPawn_Postfix(ref bool __result, RoyalTitlePermitDef __instance, Pawn pawn, Faction faction)
    {
        if (__result == false)
            return;
        
        MaxTitlePermitExtension permitExtension = __instance.GetModExtension<MaxTitlePermitExtension>();
        if (permitExtension == null) 
            return;
        
        RoyalTitleDef currentTitle = pawn.royalty.GetCurrentTitle(faction);

        if (currentTitle.seniority < __instance.minTitle.seniority ||
            currentTitle.seniority > permitExtension.maxTitle.seniority)
        {
            __result = false;
        }
    }
    
    /// <summary>
    /// Remove permits that are no longer valid due to a new title exceeding the permit's MaxTitlePermitExtension's max
    /// </summary>
    private static void RoyalTitleAwardWorker_DoAward_Postfix(Pawn pawn, Faction faction, RoyalTitleDef currentTitle, RoyalTitleDef newTitle)
    {
        foreach (FactionPermit permit in pawn.royalty.AllFactionPermits.ToList())
        {
            MaxTitlePermitExtension permitExtension = permit.Permit.GetModExtension<MaxTitlePermitExtension>();

            if (newTitle.seniority <= permitExtension?.maxTitle.seniority) continue;
            
            Messages.Message("FCP_MessagePermitLostOnPromotion".Translate(pawn, currentTitle.GetLabelFor(pawn), permit.Permit),
                MessageTypeDefOf.NeutralEvent);
            pawn.royalty.AllFactionPermits.Remove(permit);
        }
    }
    #endregion

    #region Forced TraderKindDef for PawnGroupMaker

    public static void PawnGroupKindWorker_Trader_GeneratePawns_Prefix(PawnGroupMakerParms parms, PawnGroupMaker groupMaker)
    {
        if (groupMaker is not GroupMakerWithTraderKind groupMakerWithTrader) return;
        if (groupMakerWithTrader.traderKinds.Empty())
        {
            FCPLog.Warning("A GroupMakerWithTraderKind was defined without any traderKindDefs assigned");
            return;
        }
        
        parms.traderKind = groupMakerWithTrader.traderKinds.RandomElement();
    }

    #endregion
    
    #region Custom NPC Traders

    public static bool PawnGroupKindWorker_Trader_GenerateTrader_Prefix(ref Pawn __result, PawnGroupMakerParms parms, PawnGroupMaker groupMaker, TraderKindDef traderKind)
    {
        if (groupMaker is not GroupMakerWithTraderKind groupMakerWithTrader || groupMakerWithTrader.characterDefs.Empty()) return true;
        if (!Rand.Chance(groupMakerWithTrader.characterChance)) return true;
        var list = groupMakerWithTrader.characterDefs.ToList();
        var uniqueCharTracker = UniqueCharactersTracker.Instance;
        Pawn customPawn = null;
        var faction = parms.faction;
        while (list.Count > 0)
        {
            var index = Rand.Range(0, list.Count);
            var characterCustom = list[index];
            if (!uniqueCharTracker.CharacterPawnDead(characterCustom) && !uniqueCharTracker.CharacterPawnSpawned(characterCustom))
            {
                customPawn = uniqueCharTracker.GetOrGenPawn(characterCustom, null, faction);
                break;
            } 
            list.RemoveAt(index);
        }
        if (customPawn == null) return true;
        customPawn.mindState.wantsToTradeWithColony = true;
        PawnComponentsUtility.AddAndRemoveDynamicComponents(customPawn, true);
        customPawn.trader.traderKind = traderKind;
        parms.points -= customPawn.kindDef.combatPower;
        // Skip the original method
        __result = customPawn;
        return false;
    }

    #endregion
    
    #region Add Custom Character to PawnGroupKindWorkers

    public static void PawnGroupKindWorker_Normal_GeneratePawns_Postfix(PawnGroupMakerParms parms, PawnGroupMaker groupMaker, ref List<Pawn> outPawns)
    {
        if (groupMaker is not GroupMakerWithCustomChar groupMakerWithCustomChar) return;
        if (groupMakerWithCustomChar.characterDefs.Empty())
        {
            FCPLog.Warning("A GroupMakerWithCustomChar was defined without any characterDefs assigned");
            return;
        }
        if (!Rand.Chance(groupMakerWithCustomChar.characterChance)) return;
        var list = groupMakerWithCustomChar.characterDefs.ToList();
        var uniqueCharTracker = UniqueCharactersTracker.Instance;
        Pawn customPawn = null;
        var faction = parms.faction;
        while (list.Count > 0)
        {
            var index = Rand.Range(0, list.Count);
            var characterCustom = list[index];
            if (!uniqueCharTracker.CharacterPawnDead(characterCustom) &&
                !uniqueCharTracker.CharacterPawnSpawned(characterCustom))
            {

                customPawn = uniqueCharTracker.GetOrGenPawn(characterCustom, null, faction);
                break;
            }
            list.RemoveAt(index);
        }
        if (customPawn == null) return;
        outPawns.Add(customPawn);
    }

    #endregion

    #region Faction Permanent Hostility
        
    /// <summary>
    /// Patch to change the ArePermanentEnemies result to true if they are permanent enemies because of the extension.
    /// </summary>
    public static void GoodwillSituationWorker_PermanentEnemy_ArePermanentEnemies_Postfix(Faction a, Faction b, 
        ref bool __result)
    {
        if (__result) return;
        
        ModExtension_FactionPermanentlyHostileTo aExtension = a.def.GetModExtension<ModExtension_FactionPermanentlyHostileTo>();
        ModExtension_FactionPermanentlyHostileTo bExtension = a.def.GetModExtension<ModExtension_FactionPermanentlyHostileTo>();

        // Check if either are permanently hostile with each other, but if both are null just use the existing result (false)
        __result = aExtension?.FactionIsHostileTo(b.def) ??
                   bExtension?.FactionIsHostileTo(a.def) ?? 
                   __result;
    }
    
    /// <summary>
    /// Patch so that they are unable to change goodwill after the start of the game.
    /// </summary>
    public static void Faction_CanChangeGoodwillFor_Postfix(Faction other, Faction __instance, ref bool __result)
    {
        if (__result == false)
            return;

        ModExtension_FactionPermanentlyHostileTo extension = __instance.def.GetModExtension<ModExtension_FactionPermanentlyHostileTo>();
        if (extension == null)
            return;

        __result = !extension.FactionIsHostileTo(other.def);
    }
    
    /// <summary>
    /// I think this is used in some quests to check if two factions are permanently hostile
    /// </summary>
    public static void FactionDef_PermanentlyHostileTo_Postfix(FactionDef otherFactionDef, FactionDef __instance, ref bool __result)
    {
        if (__result) return;

        ModExtension_FactionPermanentlyHostileTo extension = __instance.GetModExtension<ModExtension_FactionPermanentlyHostileTo>();
        __result = extension?.FactionIsHostileTo(otherFactionDef) ?? false;
    }
        
    /// <summary>
    /// Patches the initial goodwill which is run on faction generation or reset
    /// to return -100 if they have this extension and are in the list.
    /// </summary>
    public static bool Faction_TryMakeInitialRelationsWith_GetInitialGoodwill_Prefix(Faction a, Faction b, ref int __result)
    {
        ModExtension_FactionPermanentlyHostileTo extension = a.def.GetModExtension<ModExtension_FactionPermanentlyHostileTo>();
        if (extension == null || !extension.hostileFactionDefs.Contains(b.def)) return true;
            
        // They're hostile, so set to -100 and skip the original.
        __result = -100;
        return false;
    }

    #endregion

    #region Unique Characters

    /// <summary>
    /// Patch to leader generation to create a new leader.
    /// </summary>
    public static bool Faction_TryGenerateNewLeader_Prefix(Faction __instance, ref bool __result)
    {
        var tracker = UniqueCharactersTracker.Instance;

        var leaderDefs = CharacterRoleUtils.GetAllWithRole<CharacterRole_FactionLeader>()
            .Where(charWithRole => charWithRole.characterDef.faction == __instance.def)
            .OrderByDescending(charWithRole => charWithRole.role.seniority);
        
        foreach (CharacterDefWithRole<CharacterRole_FactionLeader> charWithRole in leaderDefs)
        {
            // Get an existing or generate pawn
            var request = new PawnGenerationRequest(charWithRole.characterDef.pawnKind, __instance); // required since we can't get it from faction manager on the other side.
            Pawn leader = tracker.GetOrGenPawn(charWithRole.characterDef, request);
            
            if (leader.Faction != __instance)
                continue; // They were likely recruited somehow.
            
            if (!charWithRole.role.PawnIsValid(leader))
                continue;
            
            charWithRole.role.ApplyRole(leader);
            
            // Skip the original method
            __result = true;
            return false;
        }

        // Probably ran out of characters, so back to random ones
        return true;
    }

    #endregion

    #region Weapon Sprite Adjustment

    static void WeaponDrawPosPatch(Pawn pawn, ref Vector3 drawPos, Rot4 facing, PawnRenderFlags flags)
    {
        if (pawn.equipment?.Primary != null)
        {
            CompPositionAttributes comp = pawn.equipment?.Primary.TryGetComp<CompPositionAttributes>();
            if (comp != null)
            {
                Vector2 offset = Vector2.zero;
                Vector2 absolute = Vector2.zero;

                Stance_Busy stance_Busy = pawn.stances?.curStance as Stance_Busy;
                if (!flags.HasFlag(PawnRenderFlags.NeverAimWeapon) && stance_Busy != null && !stance_Busy.neverAimWeapon && stance_Busy.focusTarg.IsValid)
                {
                    offset = comp.Props.DraftedDrawOffset;
                    absolute = comp.Props.DraftedDrawOffsetAbsolute;
                }
                else if (PawnRenderUtility.CarryWeaponOpenly(pawn))
                {
                    offset = comp.Props.HeldDrawOffset;
                    absolute = comp.Props.HeldDrawOffsetAbsolute;
                }

                switch (facing.AsInt)
                {
                    case 0: //north
                    case 2: //south
                        break;
                    case 1: //east
                        float tmp = offset.y;
                        offset.y = -offset.x;
                        offset.x = -tmp;
                        break;
                    case 3: //west
                        float tmp1 = offset.y;
                        offset.y = -offset.x;
                        offset.x = tmp1;
                        break;
                }

                drawPos += offset.ToVector3() + absolute.ToVector3();
            }
        }

    }

    #endregion

    #region RarityLabelPatch

    static void RarityLabelPatch(List<object> selected, ref string __result)
    {
        Thing primary = null;

        for (int index = 0; index < selected.Count; ++index)
        {
            if (selected[index] is Thing outerThing)
            {
                primary = outerThing.GetInnerIfMinified();
                break;
            }

        }

        if (primary == null) { return; }

        if (primary.TryGetComp<CompLabelColored>(out CompLabelColored comp))
        {
            __result = __result.Colorize(comp.GetRarityColor());
        }

    }

    #endregion
}