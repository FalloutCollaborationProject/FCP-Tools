using System.Collections.Concurrent;
using Verse;

namespace FCP.Core;

public static class ApparelExtensionUtility
{
    private static readonly ConcurrentDictionary<ThingDef, ApparelExtension> CachedExtensions = [];

    public static bool ShouldHideBody(this ThingDef def)
    {
        var extension = CachedExtensions.GetOrAdd(def, static d => d.GetModExtension<ApparelExtension>());
        return extension != null && extension.shouldHideBody;
    }

    public static bool ShouldHideHead(this ThingDef def)
    {
        var extension = CachedExtensions.GetOrAdd(def, static d => d.GetModExtension<ApparelExtension>());
        return extension != null && extension.shouldHideHead;
    }

    public static BodyTypeDef GetDisplayBodyType(this ThingDef def)
    {
        var extension = CachedExtensions.GetOrAdd(def, static d => d.GetModExtension<ApparelExtension>());
        return extension?.displayBodyType;
    }
}
