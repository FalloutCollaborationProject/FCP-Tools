namespace FCP.Core.Traits;

[DefOf]
public static class TraitsDefOf
{
    public static TraitDef FCP_Trait_Bloody_Mess;
    public static TraitDef FCP_Trait_Night_Person;
    public static TraitDef FCP_Trait_Solar_Powered;
    public static TraitDef FCP_Trait_Rooted;
    public static TraitDef FCP_Trait_Serendipity;
    public static TraitDef FCP_Trait_Scrapper;
    public static TraitDef FCP_Trait_Robotics_Expert;

    static TraitsDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(TraitsDefOf));
    }
}
