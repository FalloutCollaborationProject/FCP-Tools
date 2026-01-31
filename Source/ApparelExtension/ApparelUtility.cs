namespace FCP.ApparelExtensions;

public static class ApparelUtility
{
    private static readonly Dictionary<ThingDef, ApparelExtension> CachedExtensions = [];

    extension(ThingDef def)
    {
        public bool ShouldHideBody()
        {
            if (!CachedExtensions.TryGetValue(def, out ApparelExtension extension))
            {
                CachedExtensions[def] = extension = def.GetModExtension<ApparelExtension>();
            }
            return extension != null && extension.shouldHideBody;
        }

        public bool ShouldHideHead()
        {
            if (!CachedExtensions.TryGetValue(def, out ApparelExtension extension))
            {
                CachedExtensions[def] = extension = def.GetModExtension<ApparelExtension>();
            }
            return extension != null && extension.shouldHideHead;
        }
    }
}