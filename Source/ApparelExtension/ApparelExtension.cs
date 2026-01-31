using RimWorld;
using Verse;

namespace FCP.ApparelExtensions;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class ApparelExtension : DefModExtension
{
    public bool shouldHideBody;
    public bool shouldHideHead;
    public BodyTypeDef displayBodyType;
}