namespace FCP.Furniture;

public class CompUseEffect_BecomeDef : CompUseEffect
{
    public CompProperties_UseEffectBecomeDef Props => (CompProperties_UseEffectBecomeDef)props;

    private bool used;

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref used, "used", false);
    }

    public override void DoEffect(Pawn usedBy)
    {
        base.DoEffect(usedBy);

        if (used || parent.Destroyed || Props.becomeDef == null || parent.Map == null)
        {
            return;
        }

        used = true;

        IntVec3 pos = parent.Position;
        Rot4 rot = parent.Rotation;
        Map map = parent.Map;
        Faction faction = parent.Faction;

        parent.Destroy(DestroyMode.Vanish);

        Thing empty = ThingMaker.MakeThing(Props.becomeDef);
        empty.SetFactionDirect(faction);
        GenSpawn.Spawn(empty, pos, map, rot);
    }
}
