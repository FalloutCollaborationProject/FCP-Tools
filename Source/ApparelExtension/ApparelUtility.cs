using System.Collections.Concurrent;

namespace FCP.ApparelExtensions;

public static class ApparelUtility
{
    private static readonly ConcurrentDictionary<ThingDef, ApparelExtension> CachedExtensions = [];

    extension(ThingDef def)
    {
        public bool ShouldHideBody()
        {
            var extension = CachedExtensions.GetOrAdd(def, static d => d.GetModExtension<ApparelExtension>());
            return extension != null && extension.shouldHideBody;
        }

        public bool ShouldHideHead()
        {
            var extension = CachedExtensions.GetOrAdd(def, static d => d.GetModExtension<ApparelExtension>());
            return extension != null && extension.shouldHideHead;
        }
    }
}