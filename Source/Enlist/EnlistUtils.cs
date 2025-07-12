using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace  FCP.Enlist
{
    [StaticConstructorOnStartup]
    public static class EnlistUtils
    {
        static EnlistUtils()
        {
            foreach (var factionEnlistOptionsDef in DefDatabase<FactionEnlistOptionsDef>.AllDefs)
            {
                if (EnlistMod.settings.enlistStates == null) EnlistMod.settings.enlistStates = new Dictionary<string, bool>();
                if (!EnlistMod.settings.enlistStates.ContainsKey(factionEnlistOptionsDef.defName))
                {
                    EnlistMod.settings.enlistStates[factionEnlistOptionsDef.defName] = true;
                }

                if (EnlistMod.settings.enlistStates[factionEnlistOptionsDef.defName] && factionEnlistOptionsDef.autoAssignToAllFactions)
                {
                    foreach (var factionDef in DefDatabase<FactionDef>.AllDefs)
                    {
                        if (factionEnlistOptionsDef.overrideModExtension)
                        {
                            var options = factionDef.GetModExtension<FactionEnlistOptions>();
                            if (options != null)
                            {
                                factionDef.modExtensions.Remove(options);
                            }
                            var newOptions = new FactionEnlistOptions();
                            newOptions.enlistOptionsDefs = new List<FactionEnlistOptionsDef>();
                            newOptions.enlistOptionsDefs.Add(factionEnlistOptionsDef);
                            factionDef.modExtensions.Add(newOptions);
                        }
                        if (factionEnlistOptionsDef.requiredTechLevel.HasValue)
                        {
                            if (factionDef.techLevel == factionEnlistOptionsDef.requiredTechLevel.Value)
                            {
                                AssignModExtension(factionDef, factionEnlistOptionsDef);
                            }
                        }
                        else
                        {
                            AssignModExtension(factionDef, factionEnlistOptionsDef);
                        }
                    }
                }
            }
            DoDefsRemoval();
        }

        public static void ChangeRelation(this Faction faction, int baseGoodwill, Faction targetFaction = null)
        {
            if (targetFaction is null)
            {
                targetFaction = Faction.OfPlayer;
            }
            if (targetFaction.GoodwillWith(faction) != baseGoodwill)
            {
                var kind = GetKindFromBaseGoodwill(baseGoodwill);
                ChangeRelation(faction, kind, baseGoodwill, targetFaction);
            }
        }
        private static void ChangeRelation(this Faction faction, FactionRelationKind factionRelationKind, int baseGoodwill, Faction targetFaction)
        {

            var kind = faction.RelationKindWith(targetFaction);
            var factionRelation = faction.RelationWith(targetFaction);
            factionRelation.kind = factionRelationKind;
            factionRelation.baseGoodwill = baseGoodwill;
            faction.Notify_RelationKindChanged(targetFaction, kind, true, null, default, out _);

            var playerRelation = targetFaction.RelationWith(faction);
            playerRelation.kind = factionRelationKind;
            playerRelation.baseGoodwill = baseGoodwill;
            targetFaction.Notify_RelationKindChanged(faction, kind, false, null, default, out _);
        }

        public static FactionRelationKind GetKindFromBaseGoodwill(int baseGoodwill)
        {
            if (baseGoodwill <= -75)
            {
                return FactionRelationKind.Hostile;
            }
            if (baseGoodwill >= 75)
            {
                return FactionRelationKind.Ally;
            }
            return FactionRelationKind.Neutral;
        }


        public static bool PawnSatisfiesSkillRequirements(Pawn pawn, List<SkillRequirement> skillRequirements)
        {
            return FirstSkillRequirementPawnDoesntSatisfy(pawn, skillRequirements) == null;
        }

        public static SkillRequirement FirstSkillRequirementPawnDoesntSatisfy(Pawn pawn, List<SkillRequirement> skillRequirements)
        {
            if (skillRequirements == null)
            {
                return null;
            }
            for (int i = 0; i < skillRequirements.Count; i++)
            {
                if (!skillRequirements[i].PawnSatisfies(pawn))
                {
                    return skillRequirements[i];
                }
            }
            return null;
        }
        public static List<FactionEnlistOptionsDef> GetEnlistOptions(this Faction faction)
        {
            var optionDefs = new List<FactionEnlistOptionsDef>();
            if (faction != null)
            {
                FactionEnlistOptions options = faction.def.GetModExtension<FactionEnlistOptions>();
                if (options != null)
                {
                    if (!options.dontGiveEnlistmentOptions)
                    {
                        optionDefs = options.enlistOptionsDefs;
                    }
                }
            }
            return optionDefs.Where(x => EnlistMod.settings.enlistStates[x.defName]).ToList();
        }

        private static void AssignModExtension(FactionDef factionDef, FactionEnlistOptionsDef factionEnlistOptionsDef)
        {
            var options = factionDef.GetModExtension<FactionEnlistOptions>();
            if (options is null)
            {
                options = new FactionEnlistOptions();
                options.enlistOptionsDefs = new List<FactionEnlistOptionsDef>();
                if (factionDef.modExtensions is null)
                {
                    factionDef.modExtensions = new List<DefModExtension>();
                }
                factionDef.modExtensions.Add(options);
            }
            else if (options.dontGiveEnlistmentOptions || options.ignoreAutoAssignedDefs)
            {
                return;
            }
            options.enlistOptionsDefs.Add(factionEnlistOptionsDef);
        }

        public static void RemoveDef(FactionEnlistOptionsDef def)
        {
            try
            {
                if (DefDatabase<FactionEnlistOptionsDef>.AllDefsListForReading.Contains(def))
                {
                    DefDatabase<FactionEnlistOptionsDef>.AllDefsListForReading.Remove(def);
                }
            }
            catch { };
        }
        public static void DoDefsRemoval()
        {
            foreach (var enlistState in EnlistMod.settings.enlistStates)
            {
                if (!enlistState.Value)
                {
                    var defToRemove = DefDatabase<FactionEnlistOptionsDef>.GetNamedSilentFail(enlistState.Key);
                    if (defToRemove != null)
                    {
                        RemoveDef(defToRemove);
                    }
                }
            }
        }
    }
}
