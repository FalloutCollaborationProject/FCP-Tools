using RimWorld;
using Verse;

namespace FCP.Core;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class ApparelExtension : DefModExtension
{
    public bool shouldHideBody;
    public bool shouldHideHead;
    public BodyTypeDef displayBodyType;
}
