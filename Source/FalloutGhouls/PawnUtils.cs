using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace FalloutCore
{
    public static class PawnUtils
    {
        private static HashSet<string> ghoulDefNames = new HashSet<string>
        {
            "Ghoul",
            "GlowingGhoul",
            "FeralGhoul",
            "GhoulTough"
        };

        public static HashSet<string> feralGhouls = new HashSet<string>
        {
            "FeralGhoul",
        };

        public static bool IsGhoul(this Pawn pawn)
        {
            return ghoulDefNames.Contains(pawn.def.defName);
        }

        public static bool IsFeralGhoul(this Pawn pawn)
        {
            return feralGhouls.Contains(pawn.def.defName);
        }

        public static void TryHeal(Pawn pawn)
        {
            if (pawn.health.hediffSet.HasNaturallyHealingInjury())
            {
                float num3 = 8f;
                if (pawn.GetPosture() != 0)
                {
                    num3 += 4f;
                    Building_Bed building_Bed = pawn.CurrentBed();
                    if (building_Bed != null)
                    {
                        num3 += building_Bed.def.building.bed_healPerDay;
                    }
                }
                foreach (Hediff hediff3 in pawn.health.hediffSet.hediffs)
                {
                    HediffStage curStage = hediff3.CurStage;
                    if (curStage != null && curStage.naturalHealingFactor != -1f)
                    {
                        num3 *= curStage.naturalHealingFactor;
                    }
                }
                    (from x in pawn.health.hediffSet.hediffs.OfType<Hediff_Injury>()
                     where x.CanHealNaturally()
                     select x).RandomElement().Heal(num3 * pawn.HealthScale * 0.01f * pawn.GetStatValue(StatDefOf.InjuryHealingFactor));
            }
            if (pawn.health.hediffSet.HasTendedAndHealingInjury() && (pawn.needs.food == null || !pawn.needs.food.Starving))
            {
                Hediff_Injury hediff_Injury = (from x in pawn.health.hediffSet.hediffs.OfType<Hediff_Injury>()
                                               where x.CanHealFromTending()
                                               select x).RandomElement();
                float tendQuality = hediff_Injury.TryGetComp<HediffComp_TendDuration>().tendQuality;
                float num4 = GenMath.LerpDouble(0f, 1f, 0.5f, 1.5f, Mathf.Clamp01(tendQuality));
                hediff_Injury.Heal(8f * num4 * pawn.HealthScale * 0.01f * pawn.GetStatValue(StatDefOf.InjuryHealingFactor));
            }
        }

        public static Pawn GetPawnDuplicate(Pawn origin, PawnKindDef newPawnKindDef)
        {
            NameTriple nameTriple = origin.Name as NameTriple;
            Pawn newPawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(newPawnKindDef, origin.Faction, PawnGenerationContext.NonPlayer,
                fixedGender: origin.gender, fixedBirthName: nameTriple.First, fixedLastName: nameTriple.Last));

            newPawn.Name = new NameTriple(nameTriple.First, nameTriple.Nick, nameTriple.Last);
            newPawn.story.childhood = origin.story.childhood;
            newPawn.story.adulthood = origin.story.adulthood;

            if (origin.playerSettings != null)
            {
                newPawn.playerSettings = new Pawn_PlayerSettings(newPawn);
                newPawn.playerSettings.hostilityResponse = origin.playerSettings.hostilityResponse;
                newPawn.playerSettings.AreaRestrictionInPawnCurrentMap = origin.playerSettings.AreaRestrictionInPawnCurrentMap;
                newPawn.playerSettings.medCare = origin.playerSettings.medCare;
                newPawn.playerSettings.selfTend = origin.playerSettings.selfTend;
            }

            if (newPawn.foodRestriction == null) newPawn.foodRestriction = new Pawn_FoodRestrictionTracker();
            if (origin.foodRestriction?.CurrentFoodPolicy != null) newPawn.foodRestriction.CurrentFoodPolicy = origin.foodRestriction?.CurrentFoodPolicy;
            if (newPawn.outfits == null) newPawn.outfits = new Pawn_OutfitTracker();
            if (origin.outfits?.CurrentApparelPolicy != null) newPawn.outfits.CurrentApparelPolicy = origin.outfits?.CurrentApparelPolicy;
            if (newPawn.drugs == null) newPawn.drugs = new Pawn_DrugPolicyTracker();
            if (origin.drugs?.CurrentPolicy != null) newPawn.drugs.CurrentPolicy = origin.drugs?.CurrentPolicy;
            if (newPawn.timetable == null) newPawn.timetable = new Pawn_TimetableTracker(newPawn);
            if (origin.timetable?.times != null) newPawn.timetable.times = origin.timetable?.times;

            var thoughts = origin.needs?.mood?.thoughts?.memories?.Memories;
            bool isFactionLeader = origin.Faction.leader == origin;
            var relations = origin.relations?.DirectRelations;
            var relatedPawns = origin.relations?.RelatedPawns?.ToHashSet();
            foreach (var otherPawn in origin.relations.RelatedPawns)
            {
                foreach (var rel2 in origin.GetRelations(otherPawn))
                {
                    if (relations.Where(r => r.def == rel2 && r.otherPawn == otherPawn).Count() == 0)
                    {
                        if (!rel2.implied)
                        {
                            relations.Add(new DirectPawnRelation(rel2, otherPawn, 0));
                        }
                    }
                }
                relatedPawns.Add(otherPawn);
            }

            var priorities = new Dictionary<WorkTypeDef, int>();
            if (origin.workSettings != null && Traverse.Create(origin.workSettings).Field("priorities").GetValue<DefMap<WorkTypeDef, int>>() != null)
            {
                foreach (WorkTypeDef w in DefDatabase<WorkTypeDef>.AllDefs)
                {
                    priorities[w] = origin.workSettings.GetPriority(w);
                }
            }

            if (newPawn.Faction != origin.Faction)
            {
                newPawn.SetFaction(origin.Faction);
            }
            if (isFactionLeader)
            {
                newPawn.Faction.leader = newPawn;
            }

            if (newPawn.needs?.mood?.thoughts?.memories?.Memories != null)
            {
                for (int num = newPawn.needs.mood.thoughts.memories.Memories.Count - 1; num >= 0; num--)
                {
                    newPawn.needs.mood.thoughts.memories.RemoveMemory(newPawn.needs.mood.thoughts.memories.Memories[num]);
                }
            }

            if (thoughts != null)
            {
                foreach (var thought in thoughts)
                {
                    if (thought is Thought_MemorySocial && thought.otherPawn == null)
                    {
                        continue;
                    }
                    newPawn.needs.mood.thoughts.memories.TryGainMemory(thought, thought.otherPawn);
                }
            }
            newPawn.story.traits.allTraits.Clear();
            var traits = origin.story?.traits?.allTraits;
            if (traits != null)
            {
                foreach (var trait in traits)
                {
                    newPawn.story.traits.GainTrait(trait);
                }
            }

            newPawn.relations.ClearAllRelations();
            foreach (var otherPawn in relatedPawns)
            {
                if (otherPawn != null)
                {
                    foreach (var rel in otherPawn.relations.DirectRelations)
                    {
                        if (origin.Name == rel.otherPawn?.Name)
                        {
                            rel.otherPawn = newPawn;
                        }
                    }
                }
            }

            foreach (var otherPawn in relatedPawns)
            {
                if (otherPawn != null)
                {
                    foreach (var rel in relations)
                    {
                        foreach (var rel2 in otherPawn.relations.DirectRelations)
                        {
                            if (rel.def == rel2.def && rel2.otherPawn?.Name == newPawn.Name)
                            {
                                rel2.otherPawn = newPawn;
                            }
                        }
                    }
                }
            }

            foreach (var rel in relations)
            {
                if (rel.otherPawn != null)
                {
                    var oldRelation = rel.otherPawn.relations.DirectRelations.Where(r => r.def == rel.def && r.otherPawn.Name == newPawn.Name).FirstOrDefault();
                    if (oldRelation != null)
                    {
                        oldRelation.otherPawn = newPawn;
                    }
                }
                newPawn.relations.AddDirectRelation(rel.def, rel.otherPawn);
            }

            var skills = origin.skills.skills;
            newPawn.skills.skills.Clear();
            if (skills != null)
            {
                foreach (var skill in skills)
                {
                    var newSkill = new SkillRecord(newPawn, skill.def);
                    newSkill.passion = skill.passion;
                    newSkill.levelInt = skill.levelInt;
                    newSkill.xpSinceLastLevel = skill.xpSinceLastLevel;
                    newSkill.xpSinceMidnight = skill.xpSinceMidnight;
                    newPawn.skills.skills.Add(newSkill);
                }
            }

            if (newPawn.workSettings == null) newPawn.workSettings = new Pawn_WorkSettings();
            newPawn.Notify_DisabledWorkTypesChanged();
            if (priorities != null)
            {
                foreach (var priority in priorities)
                {
                    newPawn.workSettings.SetPriority(priority.Key, priority.Value);
                }
            }

            var apparels = origin.apparel?.WornApparel ?? new List<Apparel>();
            newPawn.apparel.DestroyAll();
            for (int num = apparels.Count - 1; num >= 0; num--)
            {
                var apparel = apparels[num];
                origin.apparel.Remove(apparel);
                newPawn.apparel.Wear(apparel);
            }

            var equipments = origin.equipment?.AllEquipmentListForReading ?? new List<ThingWithComps>();
            newPawn.equipment.DestroyAllEquipment();
            for (int num = equipments.Count - 1; num >= 0; num--)
            {
                var equipment = equipments[num];
                origin.equipment.Remove(equipment);
                newPawn.equipment.AddEquipment(equipment);
            }

            var inventoryThings = origin.inventory?.innerContainer?.ToList() ?? new List<Thing>();
            newPawn.inventory.DestroyAll();
            for (int num = inventoryThings.Count - 1; num >= 0; num--)
            {
                origin.inventory.innerContainer.Remove(inventoryThings[num]);
                newPawn.inventory.TryAddItemNotForSale(inventoryThings[num]);
            }

            if (ModLister.RoyaltyInstalled && origin.royalty != null)
            {
                var royalTitles = origin.royalty?.AllTitlesForReading;
                var favor = Traverse.Create(origin.royalty).Field("favor").GetValue<Dictionary<Faction, int>>();
                var heirs = Traverse.Create(origin.royalty).Field("heirs").GetValue<Dictionary<Faction, Pawn>>();
                var factionPermits = Traverse.Create(origin.royalty).Field("factionPermits").GetValue<List<FactionPermit>>();
                var permitPoints = Traverse.Create(origin.royalty).Field("permitPoints").GetValue<Dictionary<Faction, int>>();
                List<Thing> bondedThings = new List<Thing>();
                foreach (var map in Find.Maps)
                {
                    foreach (var thing in map.listerThings.AllThings)
                    {
                        var comp = thing.TryGetComp<CompBladelinkWeapon>();
                        if (comp != null && comp.CodedPawn == origin)
                        {
                            bondedThings.Add(thing);
                        }
                    }
                    foreach (var gear in origin.apparel?.WornApparel)
                    {
                        var comp = gear.TryGetComp<CompBladelinkWeapon>();
                        if (comp != null && comp.CodedPawn == origin)
                        {
                            bondedThings.Add(gear);
                        }
                    }
                    foreach (var gear in origin.equipment?.AllEquipmentListForReading)
                    {
                        var comp = gear.TryGetComp<CompBladelinkWeapon>();
                        if (comp != null && comp.CodedPawn == origin)
                        {
                            bondedThings.Add(gear);
                        }
                    }
                    foreach (var gear in origin.inventory?.innerContainer)
                    {
                        var comp = gear.TryGetComp<CompBladelinkWeapon>();
                        if (comp != null && comp.CodedPawn == origin)
                        {
                            bondedThings.Add(gear);
                        }
                    }
                }

                if (newPawn.royalty == null) newPawn.royalty = new Pawn_RoyaltyTracker(newPawn);
                if (royalTitles != null)
                {
                    foreach (var title in royalTitles)
                    {
                        newPawn.royalty.SetTitle(title.faction, title.def, false, false, false);
                    }
                }
                if (heirs != null)
                {
                    foreach (var heir in heirs)
                    {
                        newPawn.royalty.SetHeir(heir.Value, heir.Key);
                    }
                }

                if (favor != null)
                {
                    foreach (var fav in favor)
                    {
                        newPawn.royalty.SetFavor(fav.Key, fav.Value);
                    }
                }

                if (bondedThings != null)
                {
                    foreach (var bonded in bondedThings)
                    {
                        var comp = bonded.TryGetComp<CompBladelinkWeapon>();
                        if (comp != null)
                        {
                            comp.CodeFor(newPawn);
                        }
                    }
                }
                if (factionPermits != null)
                {
                    Traverse.Create(newPawn.royalty).Field("factionPermits").SetValue(factionPermits);
                }
                if (permitPoints != null)
                {
                    Traverse.Create(newPawn.royalty).Field("permitPoints").SetValue(permitPoints);
                }
            }
            return newPawn;
        }
    }
}