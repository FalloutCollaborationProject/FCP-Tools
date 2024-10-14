namespace FCP.Core
{
    public class CompProperties_FlyingPawn : CompProperties
    {
        public readonly float evadeChanceWhenFlying = 1f;
        
        public List<GraphicData> flyingBodyGraphicData;
        public List<GraphicData> flyingFemaleBodyGraphicData;
        public AttackDamageFactor attackDamageFactor;
        public SoundDef soundOnFly;
        
        public bool attackEnemiesMasterAttacking;
        public float flyingMoveSpeedMultiplier;
        public float flyWhenWanderingChance;
        public bool flyWhenFleeing;
        public bool flyWhenHunting;
        
        public CompProperties_FlyingPawn()
        {
            compClass = typeof(CompFlyingPawn);
        }

        public override void ResolveReferences(ThingDef parentDef)
        {
            base.ResolveReferences(parentDef);
            if (flyingBodyGraphicData != null)
            {
                foreach (GraphicData graphicData in flyingBodyGraphicData)
                {
                    graphicData.graphicClass = typeof(Graphic_Multi);
                }
            }

            if (flyingFemaleBodyGraphicData == null) return;
            foreach (GraphicData graphicData in flyingFemaleBodyGraphicData)
            {
                graphicData.graphicClass = typeof(Graphic_Multi);
            }
        }
    }   
}