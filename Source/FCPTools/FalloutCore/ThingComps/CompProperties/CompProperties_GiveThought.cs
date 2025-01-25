namespace FCP.Core
{
    public class CompProperties_GiveThought : CompProperties
    {
        public ThoughtDef thoughtDef;
        public int radius = 0;
        public bool enableInInventory = false;

        public CompProperties_GiveThought() => this.compClass = typeof(CompGiveThought);
    }
}
