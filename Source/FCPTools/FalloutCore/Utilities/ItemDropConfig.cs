namespace FCP.Core
{
    public class ItemDropConfig
    {
        public ThingDef thingDef;
        public IntRange countRange = new IntRange(1, 1);
        public float chance = 1f;
        public float weight = 1f;
    }
}