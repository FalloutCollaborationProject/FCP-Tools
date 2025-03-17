namespace FCP.Core;

public class ModExtension_LegendaryEffect : DefModExtension
{
    public List<LegendaryEffectDef> LegendaryEffects;

    public static ModExtension_LegendaryEffect RandomLegendaryFor(ThingDef thingDef)
    {
        ModExtension_LegendaryEffect ext = new ModExtension_LegendaryEffect();
        ext.AddNewLegendaryEffectFor(thingDef);

        return ext;
    }

    public void AddNewLegendaryEffectFor(ThingDef thingDef)
    {
        var allDefs = DefDatabase<LegendaryEffectDef>.AllDefsListForReading;
        LegendaryEffectDef effect = allDefs.Where(def => def.IsForApparel == thingDef.IsApparel || def.IsForWeapon == thingDef.IsWeapon).RandomElement();
        if (LegendaryEffects.NullOrEmpty())
        {
            LegendaryEffects = new List<LegendaryEffectDef>();
        }
        LegendaryEffects.Add(effect);
    }
}
