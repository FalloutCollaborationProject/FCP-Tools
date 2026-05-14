using UnityEngine;
using Verse.AI;

namespace FCP.Core.WeaponCondition;

public class CompProperties_ApparelBench : CompProperties
{
    public float drawOffsetX = 0f;
    public float drawOffsetZ = 0.15f;

    public CompProperties_ApparelBench() => compClass = typeof(CompApparelBench);
}

public class CompApparelBench : ThingComp, IThingHolder
{
    private ThingOwner<Thing> slot;
    private Quaternion cachedRot;
    private Vector3 cachedOffset;

    public CompProperties_ApparelBench Props => (CompProperties_ApparelBench)props;

    public Thing LoadedApparel => slot.Count > 0 ? slot[0] : null;

    public CompApparelBench() => slot = new ThingOwner<Thing>(this, oneStackOnly: true);

    public ThingOwner GetDirectlyHeldThings() => slot;

    public void GetChildHolders(List<IThingHolder> outChildren) =>
        ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, slot);

    public override void PostExposeData()
    {
        Scribe_Deep.Look(ref slot, "apparelSlot", this);
    }

    public override void PostDestroy(DestroyMode mode, Map previousMap)
    {
        slot.TryDropAll(parent.Position, previousMap, ThingPlaceMode.Near);
        base.PostDestroy(mode, previousMap);
    }

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        cachedRot = Quaternion.AngleAxis(parent.Rotation.AsAngle, Vector3.up);
        cachedOffset = cachedRot * new Vector3(Props.drawOffsetX, 0f, Props.drawOffsetZ);
        if (LoadedApparel != null)
            parent.Map.dynamicDrawManager.RegisterDrawable(parent);
    }

    public void Load(Thing apparel)
    {
        slot.TryAdd(apparel);
        parent.Map.dynamicDrawManager.RegisterDrawable(parent);
    }

    public override void PostDraw()
    {
        if (LoadedApparel == null) return;
        Graphic graphic = LoadedApparel.Graphic;
        Vector3 pos = parent.DrawPos + cachedOffset;
        pos.y += Altitudes.AltInc;
        Matrix4x4 matrix = Matrix4x4.TRS(pos, cachedRot, graphic.drawSize.ToVector3());
        Graphics.DrawMesh(MeshPool.plane10, matrix, graphic.MatSingle, 0);
    }

    public override string CompInspectStringExtra()
    {
        if (LoadedApparel == null)
            return "FCP_ApparelBench_Empty".Translate();
        int pct = Mathf.RoundToInt(100f * LoadedApparel.HitPoints / (float)LoadedApparel.MaxHitPoints);
        return "FCP_ApparelBench_Loaded".Translate(LoadedApparel.LabelShortCap + $" ({pct}%)");
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        if (LoadedApparel == null)
        {
            yield return new Command_Action
            {
                defaultLabel = "FCP_ApparelBench_Load".Translate(),
                defaultDesc = "FCP_ApparelBench_Load_Desc".Translate(),
                icon = TexButton.Plus,
                action = OpenLoadMenu
            };
        }
        else
        {
            yield return new Command_Action
            {
                defaultLabel = "FCP_ApparelBench_Eject".Translate(LoadedApparel.LabelShortCap),
                defaultDesc = "FCP_ApparelBench_Eject_Desc".Translate(),
                icon = (Texture2D)LoadedApparel.Graphic.MatSingle.mainTexture,
                action = Eject
            };
        }
    }

    private void OpenLoadMenu()
    {
        List<FloatMenuOption> opts = new List<FloatMenuOption>();
        foreach (Pawn p in parent.Map.mapPawns.FreeColonists)
        {
            foreach (Apparel a in p.apparel.WornApparel)
                TryAddOption(opts, a, p);
            foreach (Thing t in p.inventory.innerContainer)
            {
                if (t is Apparel a)
                    TryAddOption(opts, a, p);
            }
        }
        foreach (Thing t in parent.Map.listerThings.ThingsInGroup(ThingRequestGroup.Apparel))
        {
            if (t is Apparel a && t.IsInAnyStorage())
                TryAddOption(opts, a, null);
        }
        if (opts.Count == 0)
        {
            Messages.Message("FCP_ApparelBench_NoApparel".Translate(), MessageTypeDefOf.RejectInput, historical: false);
            return;
        }
        Find.WindowStack.Add(new FloatMenu(opts));
    }

    private void TryAddOption(List<FloatMenuOption> opts, Apparel apparel, Pawn pawn)
    {
        if (!apparel.def.defName.StartsWith("FCP_")) return;
        int pct = Mathf.RoundToInt(100f * apparel.HitPoints / (float)apparel.MaxHitPoints);
        string label = pawn != null
            ? $"{apparel.LabelShortCap} ({pct}%) [{pawn.LabelShort}]"
            : $"{apparel.LabelShortCap} ({pct}%)";
        opts.Add(new FloatMenuOption(label, () => QueueLoadJob(apparel)));
    }

    private void QueueLoadJob(Apparel apparel)
    {
        if (LoadedApparel != null) return;

        Pawn owningPawn = (apparel.ParentHolder as Pawn_ApparelTracker)?.pawn
            ?? (apparel.ParentHolder as Pawn_InventoryTracker)?.pawn;

        Pawn hauler = owningPawn;
        if (hauler == null || hauler.Downed || !hauler.CanReach(parent, PathEndMode.InteractionCell, Danger.Deadly))
        {
            hauler = null;
            float bestDist = float.MaxValue;
            foreach (Pawn p in parent.Map.mapPawns.FreeColonistsSpawned)
            {
                if (p.Downed || !p.CanReach(parent, PathEndMode.InteractionCell, Danger.Deadly))
                    continue;
                float dist = p.Position.DistanceToSquared(apparel.Position);
                if (dist < bestDist) { bestDist = dist; hauler = p; }
            }
        }

        if (hauler == null)
        {
            Messages.Message("FCP_ApparelBench_NoHauler".Translate(), MessageTypeDefOf.RejectInput, historical: false);
            return;
        }

        hauler.jobs.StartJob(
            JobMaker.MakeJob(WeaponConditionDefOf.FCP_Job_LoadApparelToBench, parent, apparel),
            JobCondition.InterruptForced);
    }

    private void Eject()
    {
        slot.TryDropAll(parent.InteractionCell, parent.Map, ThingPlaceMode.Near);
    }
}
