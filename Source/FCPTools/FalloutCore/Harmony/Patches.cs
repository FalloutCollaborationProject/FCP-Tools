using HarmonyLib;
using RimWorld.Planet;
using Verse.AI;

namespace FCP.Core
{
    [StaticConstructorOnStartup]
    public static class Patches
    {
        static Patches()
        {
            Harmony harmony = new ("FCP.Core.PatchesUwU");
            
            harmony.Patch(original: AccessTools.Method(typeof(WildAnimalSpawner), "CommonalityOfAnimalNow"),
                postfix: new HarmonyMethod(typeof(Patches), nameof(WildAnimalSpawnerCommonalityOfAnimalNow_Postfix)));
            
            harmony.Patch(original: AccessTools.Method(typeof(TraderCaravanUtility), "GetTraderCaravanRole"),
                postfix: new HarmonyMethod(typeof(Patches), nameof(TraderCaravanUtilityGetTraderCaravanRole_Postfix)));
            
            harmony.Patch(original: AccessTools.Method(typeof(Pawn_GuestTracker), "RandomizeJoinStatus"),
                postfix: new HarmonyMethod(typeof(Patches), nameof(Pawn_GuestTrackerRandomizeJoinStatus_Postfix)));
            
            harmony.Patch(original: AccessTools.Method(typeof(Pawn), "PreTraded"),
                postfix: new HarmonyMethod(typeof(Patches), nameof(PawnPreTraded_Postfix)));
            
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
            
            harmony.Patch(original: AccessTools.Method(typeof(IdeoGenerator), "MakeFixedIdeo"),
                postfix: new HarmonyMethod(typeof(Patches), nameof(IdeoGeneratorMakeFixedIdeo_Postfix)));
            
            harmony.Patch(original: AccessTools.Method(typeof(IdeoFoundation), "RandomizeIcon"),
                prefix: new HarmonyMethod(typeof(Patches), nameof(IdeoFoundationRandomizeIcon_Prefix)));
            
            harmony.Patch(original: AccessTools.Method(typeof(IdeoFoundation), "InitPrecepts"),
                postfix: new HarmonyMethod(typeof(Patches), nameof(IdeoFoundationInitPrecepts_Postfix)));
            
            // harmony.Patch(original: AccessTools.Method(typeof(IdeoFoundation), "AddSpecialPrecepts"),
            //     prefix: new HarmonyMethod(typeof(Patches), nameof(IdeoFoundationAddSpecialPrecepts_Prefix)));
        }
        
        /// <summary>
        /// Ensures certain environmental conditions are met before a pawn can be (naturally) spawned on the map.
        /// </summary>
        public static void WildAnimalSpawnerCommonalityOfAnimalNow_Postfix(PawnKindDef def, ref Map ___map, ref float __result)
        {
            ModExtension_BiomeFeatureRequirements extension = def.race.GetModExtension<ModExtension_BiomeFeatureRequirements>();

            if (extension is null) return;
            bool hasNoCaves = extension.requireCaves && !Find.World.HasCaves(___map.Tile);
            bool hasNoCoast = extension.requireCoast && !Find.World.CoastDirectionAt(___map.Tile).IsValid;
            bool hasNoHills = extension.requireHills && Find.WorldGrid[___map.Tile].hilliness == Hilliness.Flat;
            bool hasNoRiver = extension.requireRiver && Find.WorldGrid[___map.Tile].Rivers == null;

            if (hasNoCaves || hasNoCoast || hasNoHills || hasNoRiver)
            {
                __result = 0;
            }
        }
        
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
        
        /// <summary>
        /// Modifies how damage is applied to a specific body part of a Pawn.
        /// When the instigator (the entity dealing the damage) is a flying pawn.
        /// </summary>
        public static void DamageWorker_AddInjuryApplyDamageToPart_Prefix(ref DamageInfo dinfo, Pawn pawn, DamageWorker.DamageResult result)
        {
            Thing instigator = dinfo.Instigator;
            
            if (instigator is not Pawn instigatorPawn || !instigatorPawn.IsFlyingPawn(out CompFlyingPawn flyingComp)) return;
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
        public static void JobGiver_AIDefendPawnFindAttackTarget_Postfix(JobGiver_AIDefendPawn __instance, ref Thing __result, Pawn pawn)
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
            }
        }
        
        /*public static void IdeoFoundationAddSpecialPrecepts_Prefix(IdeoGenerationParms parms, Ideo ___ideo, IdeoFoundation __instance)
        {
            ModExtension_FixedIdeo extension = parms.forFaction.GetModExtension<ModExtension_FixedIdeo>();
            
            if (!parms.fixedIdeo || extension == null) return;
            foreach (var preceptDef in extension.requiredPrecepts)
            {
                if (!__instance.CanAddForFaction(preceptDef, parms.forFaction, parms.disallowedPrecepts, true, false,
                        false, parms.classicExtra))
                {
                    FCPLog.Error($"Couldn't add {preceptDef.defName} to ideo {parms.name} for faction {parms.forFaction.defName}, maybe there is an imcompatible precept required by a meme?");
                    continue;
                }
                ___ideo.AddPrecept(PreceptMaker.MakePrecept(preceptDef));
            }
        }*/
    }
}