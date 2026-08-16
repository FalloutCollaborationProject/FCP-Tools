using Verse.Sound;

namespace FCP.Core.GECK;

public class CompGECKActivator : ThingComp
{
    public CompProperties_GECKActivator Props => (CompProperties_GECKActivator)props;

    private bool activated;

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref activated, "geckActivated", false);
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        foreach (Gizmo gizmo in base.CompGetGizmosExtra())
        {
            yield return gizmo;
        }

        if (activated)
        {
            yield break;
        }

        yield return new Command_Action
        {
            defaultLabel = "FCP_GECK_Activate".Translate(),
            defaultDesc = "FCP_GECK_ActivateDesc".Translate(),
            icon = parent.def.uiIcon,
            action = ConfirmActivate
        };
    }

    private void ConfirmActivate()
    {
        Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
            "FCP_GECK_ConfirmActivate".Translate(),
            Activate,
            destructive: true));
    }

    private void Activate()
    {
        if (activated)
        {
            return;
        }

        activated = true;

        Map map = parent.Map;
        IntVec3 center = parent.Position;
        TerrainDef terrain = Props.targetTerrain ?? TerrainDefOf.SoilRich;

        foreach (IntVec3 cell in GenRadial.RadialCellsAround(center, Props.radius, true))
        {
            if (!cell.InBounds(map))
            {
                continue;
            }

            Building edifice = cell.GetEdifice(map);
            if (edifice != null && edifice != parent)
            {
                continue;
            }

            TerrainDef current = map.terrainGrid.TerrainAt(cell);
            if (current == null || current.fertility >= terrain.fertility)
            {
                continue;
            }

            if (!current.affordances.Contains(TerrainAffordanceDefOf.Light))
            {
                continue;
            }

            map.terrainGrid.SetTerrain(cell, terrain);
        }

        FleckMaker.Static(center, map, FleckDefOf.LightningGlow, Props.radius * 2f);
        if (Props.activationSound != null)
        {
            SoundStarter.PlayOneShot(Props.activationSound, SoundInfo.InMap(new TargetInfo(center, map), MaintenanceType.None));
        }

        if (Props.activationLetterDef != null)
        {
            Find.LetterStack.ReceiveLetter(
                "FCP_GECK_LetterLabel".Translate(),
                "FCP_GECK_LetterText".Translate(),
                Props.activationLetterDef,
                new TargetInfo(center, map));
        }

        parent.Destroy(DestroyMode.Vanish);
    }
}
