using JetBrains.Annotations;
using Verse;

namespace FCP.Core;

[UsedImplicitly]
public class ItemDrop
{
    public ThingDef thingDef;
    public IntRange countRange = new IntRange(1,1);
    public float chance = 1f;
}