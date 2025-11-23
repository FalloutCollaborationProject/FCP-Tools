using System.Text;
using System.Text.RegularExpressions;
namespace FCP.Core;

public class LegendaryEffectGameTracker : GameComponent
{
    public static Dictionary<Thing, List<LegendaryEffectDef>> EffectsDict = new();
    private static List<Thing> _effectsKeys;
    private static List<List<LegendaryEffectDef>> _effectsValues;
    public LegendaryEffectGameTracker(Game game)
    {
        
    }

    public override void ExposeData()
    {
        base.ExposeData();
        if (Scribe.mode == LoadSaveMode.Saving)
        {
            _effectsKeys = [];
            _effectsValues = [];

            foreach (var kvp in EffectsDict)
            {
                _effectsKeys.Add(kvp.Key);
                _effectsValues.Add(kvp.Value);
            }
        }
        else if (Scribe.mode == LoadSaveMode.LoadingVars)
        {
            _effectsKeys = null;
            _effectsValues = null;
        }

        Scribe_Collections.Look(ref _effectsKeys, "EffectsDict_keys", LookMode.Reference);
        Scribe_Collections.Look(ref _effectsValues, "EffectsDict_values", LookMode.Def);

        if (Scribe.mode != LoadSaveMode.PostLoadInit) return;
        EffectsDict.Clear();

        if (_effectsKeys == null || _effectsValues == null)
        {
            FCPLog.Error("Legendary Effect Tracker - Failed to load EffectsDict: keys or values were null.");
            return;
        }
        if (_effectsValues.Count != _effectsKeys.Count)
        {
            FCPLog.Error("Legendary Effect Tracker - Failed to load EffectsDict: Key/Value count mismatched.");
            return;
        }
        for (int i = 0; i < _effectsKeys.Count; i++)
        {
            if (_effectsKeys[i] == null)
            {
                FCPLog.Error($"Legendary Effect Tracker - error loading EffectsDict: key at index {i} was null. Trying to continue.");
            }else{
                EffectsDict[_effectsKeys[i]] = _effectsValues[i];
            }
        }
    }

    public static void AddNewLegendaryEffectFor(Thing thing)
    {
        List<LegendaryEffectDef> AllDefs = DefDatabase<LegendaryEffectDef>.AllDefsListForReading;
        LegendaryEffectDef effect;

        if (thing.def.IsApparel)
        {
            effect = AllDefs.Where(def => def.IsForApparel).RandomElement();
        }
        else if (thing.def.IsWeapon)
        {
            if (thing.def.weaponClasses.Any(cls => cls.defName.ToLower().Contains("melee")))
            {
                effect = AllDefs.Where(def => def.IsForWeapon && def.IsForMelee).RandomElement();
            }
            else
            {
                effect = AllDefs.Where(def => def.IsForWeapon && !def.IsForMelee).RandomElement();
            }
        }
        else
        {
            return;
        }

        if (!EffectsDict.TryGetValue(thing, out var effects))
            effects = [];

        effects.Add(effect);

        EffectsDict.SetOrAdd(thing, effects);
    }

    private static void ClearEffectsFor(Thing thing)
    {
        if (!EffectsDict.TryGetValue(thing, out var effects))
            effects = [];

        effects.Clear();

        EffectsDict.SetOrAdd(thing, effects);
    }

    public static bool HasEffect(Thing thing)
    {
        return EffectsDict.ContainsKey(thing) && EffectsDict[thing].Count > 0;
    }

    public static List<LegendaryEffectDef> GetEffectsFor(Thing thing)
    {
        return !EffectsDict.TryGetValue(thing, out List<LegendaryEffectDef> value) ? [] : value;
    }

    public static void Reroll(Thing thing)
    {
        if (!HasEffect(thing))
            return;

        ClearEffectsFor(thing);
        AddNewLegendaryEffectFor(thing);
    }

    public static string GetEffectDescription(Thing thing)
    {
        if (!HasEffect(thing))
            return null;

        StringBuilder body = new();
        foreach (LegendaryEffectDef effect in EffectsDict[thing])
        {
            body.AppendLine($" - {effect.LabelCap} - {effect.description}");
        }

        return RemoveEmptyLines(body.ToString());
    }

    public static string RemoveEmptyLines(string lines)
    {
        return Regex.Replace(lines, @"^\s*$\n|\r", string.Empty, RegexOptions.Multiline).TrimEnd();
    }

    public static void MakeChangeEffectFloatMenu(Thing thing)
    {
        if (!EffectsDict.TryGetValue(thing, out List<LegendaryEffectDef> effects))
            effects = [];

        List<LegendaryEffectDef> AllDefs = DefDatabase<LegendaryEffectDef>.AllDefsListForReading;
        List<FloatMenuOption> options = [];
        IEnumerable<LegendaryEffectDef> validEffects;
        
        if (thing.def.IsApparel)
        {
            validEffects = AllDefs.Where(def => def.IsForApparel);
        }
        else if (thing.def.IsWeapon)
        {
            validEffects = thing.def.weaponClasses.Any(cls => cls.defName.ToLower().Contains("melee"))
                ? AllDefs.Where(def => def.IsForWeapon && def.IsForMelee)
                : AllDefs.Where(def => def.IsForWeapon);
        }
        else
        {
            return;
        }

        foreach (LegendaryEffectDef effect in validEffects.Where(eff => !effects.Contains(eff)))
        {
            options.Add(
                new FloatMenuOption(
                    effect.LabelCap,
                    () =>
                    {
                        effects.Clear();
                        effects.Add(effect);
                    }
                )
            );
        }
        Find.WindowStack.Add(new FloatMenu(options));
        EffectsDict.SetOrAdd(thing, effects);
    }

    public struct ThingAndEffect
    {
        public Thing thing;
        public LegendaryEffectDef effect;
    }

    public static List<ThingAndEffect> EffectsForPawn(Pawn pawn)
    {
        List<ThingAndEffect> output = [];

        if (pawn == null)
            return output;

        if (pawn.apparel != null)
        {
            foreach (Apparel apparel in pawn.apparel.UnlockedApparel)
            {
                output.AddRange(GetEffectsFor(apparel)
                    .Select(eff => 
                        new ThingAndEffect { thing = apparel, effect = eff }));
            }
        }

        if (pawn.equipment is { Primary: not null })
        {
            output.AddRange(GetEffectsFor(pawn.equipment.Primary)
                .Select(eff => 
                    new ThingAndEffect { thing = pawn.equipment.Primary, effect = eff }));
        }

        return output;
    }

    public static float CooldownModifier(Pawn pawn)
    {
        float modifier = 1f;

        foreach (ThingAndEffect thingAndEffect in EffectsForPawn(pawn))
        {
            modifier *= thingAndEffect.effect.VATS_Multiplier;
        }

        return modifier;
    }
}