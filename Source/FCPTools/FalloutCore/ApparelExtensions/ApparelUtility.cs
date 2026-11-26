using Verse;
using Waylith.Injection;

namespace FCP.Core;

[StaticConstructorOnStartup]
public static class ApparelExtensionUtility
{
    // Waylith injects a real ApparelExtension field into ThingDef and replaces these
    // accessor bodies, so the throws should never happen.
    extension(ThingDef def)
    {
        [InjectedField]
        private ApparelExtension P_ApparelExtension
        {
            get => throw new InvalidOperationException("ApparelExtensionUtility: Waylith field injection is not active.");
            set => throw new InvalidOperationException("ApparelExtensionUtility: Waylith field injection is not active.");
        }
    }
    
    static ApparelExtensionUtility()
    {
        foreach (ThingDef def in DefDatabase<ThingDef>.AllDefsListForReading)
            def.P_ApparelExtension = def.GetModExtension<ApparelExtension>();
    }

    extension(ThingDef def)
    {
        public bool ShouldHideBody() => def.P_ApparelExtension?.shouldHideBody ?? false;
        public bool ShouldHideHead() => def.P_ApparelExtension?.shouldHideHead ?? false;
        public BodyTypeDef GetDisplayBodyType() => def.P_ApparelExtension?.displayBodyType;
    }
}
