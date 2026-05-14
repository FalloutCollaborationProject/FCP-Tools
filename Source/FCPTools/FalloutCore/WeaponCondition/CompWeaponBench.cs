using UnityEngine;
using Verse.AI;

namespace FCP.Core.WeaponCondition;

public class CompProperties_WeaponBench : CompProperties
{
    public float drawOffsetX = 0f;
    public float drawOffsetZ = 0.15f;

    public CompProperties_WeaponBench() => compClass = typeof(CompWeaponBench);
}

public class CompWeaponBench : ThingComp, IThingHolder
{
    private ThingOwner<Thing> slot;
    private Quaternion cachedRot;
    private Vector3 cachedOffset;

    public CompProperties_WeaponBench Props => (CompProperties_WeaponBench)props;

    public Thing LoadedWeapon => slot.Count > 0 ? slot[0] : null;

    public CompWeaponBench() => slot = new ThingOwner<Thing>(this, oneStackOnly: true);

    public ThingOwner GetDirectlyHeldThings() => slot;

    public void GetChildHolders(List<IThingHolder> outChildren) =>
        ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, slot);

    public override void PostExposeData()
    {
        Scribe_Deep.Look(ref slot, "weaponSlot", this);
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
            slot ??= new ThingOwner<Thing>(this, oneStackOnly: true);
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
        if (LoadedWeapon != null)
            parent.Map.dynamicDrawManager.RegisterDrawable(parent);
    }

    public void Load(Thing weapon)
    {
        slot.TryAdd(weapon);
        parent.Map.dynamicDrawManager.RegisterDrawable(parent);
    }

    public override void PostDraw()
    {
        if (LoadedWeapon == null) return;
        Graphic graphic = LoadedWeapon.Graphic;
        Vector3 pos = parent.DrawPos + cachedOffset;
        pos.y += Altitudes.AltInc;
        Matrix4x4 matrix = Matrix4x4.TRS(pos, cachedRot, graphic.drawSize.ToVector3());
        Graphics.DrawMesh(MeshPool.plane10, matrix, graphic.MatSingle, 0);
    }

    public override string CompInspectStringExtra()
    {
        if (LoadedWeapon == null)
            return "FCP_RepairBench_Empty".Translate();
        CompWeaponCondition cond = ((ThingWithComps)LoadedWeapon).GetComp<CompWeaponCondition>();
        return "FCP_RepairBench_Loaded".Translate(LoadedWeapon.LabelShortCap + $" ({cond.Condition:F0}/100)");
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        if (LoadedWeapon == null)
        {
            yield return new Command_Action
            {
                defaultLabel = "FCP_RepairBench_Load".Translate(),
                defaultDesc = "FCP_RepairBench_Load_Desc".Translate(),
                icon = TexButton.Plus,
                action = OpenLoadMenu
            };
        }
        else
        {
            yield return new Command_Action
            {
                defaultLabel = "FCP_RepairBench_Eject".Translate(LoadedWeapon.LabelShortCap),
                defaultDesc = "FCP_RepairBench_Eject_Desc".Translate(),
                icon = (Texture2D)LoadedWeapon.Graphic.MatSingle.mainTexture,
                action = Eject
            };
        }
    }

    private void OpenLoadMenu()
    {
        List<FloatMenuOption> opts = new List<FloatMenuOption>();
        foreach (Pawn p in parent.Map.mapPawns.FreeColonists)
        {
            ThingWithComps primary = p.equipment.Primary;
            if (primary != null)
                TryAddWeaponOption(opts, primary, p);
            foreach (Thing t in p.inventory.innerContainer)
            {
                if (t is ThingWithComps twc)
                    TryAddWeaponOption(opts, twc, p);
            }
        }
        foreach (Thing t in parent.Map.listerThings.ThingsInGroup(ThingRequestGroup.Weapon))
        {
            if (t is ThingWithComps twc && twc.IsInAnyStorage())
                TryAddWeaponOption(opts, twc, null);
        }
        if (opts.Count == 0)
        {
            Messages.Message("FCP_RepairBench_NoWeapons".Translate(), MessageTypeDefOf.RejectInput, historical: false);
            return;
        }
        Find.WindowStack.Add(new FloatMenu(opts));
    }

    private void TryAddWeaponOption(List<FloatMenuOption> opts, ThingWithComps weapon, Pawn pawn)
    {
        CompWeaponCondition cond = weapon.GetComp<CompWeaponCondition>();
        if (cond == null) return;
        string label = pawn != null
            ? $"{weapon.LabelShortCap} ({cond.Condition:F0}/100) [{pawn.LabelShort}]"
            : $"{weapon.LabelShortCap} ({cond.Condition:F0}/100)";
        opts.Add(new FloatMenuOption(label, () => QueueLoadJob(weapon)));
    }

    private void QueueLoadJob(ThingWithComps weapon)
    {
        if (LoadedWeapon != null) return;

        Pawn owningPawn = (weapon.ParentHolder as Pawn_EquipmentTracker)?.pawn
            ?? (weapon.ParentHolder as Pawn_InventoryTracker)?.pawn;

        Pawn hauler = owningPawn;
        if (hauler == null || hauler.Downed || !hauler.CanReach(parent, PathEndMode.InteractionCell, Danger.Deadly))
        {
            hauler = null;
            float bestDist = float.MaxValue;
            foreach (Pawn p in parent.Map.mapPawns.FreeColonistsSpawned)
            {
                if (p.Downed || !p.CanReach(parent, PathEndMode.InteractionCell, Danger.Deadly))
                    continue;
                float dist = p.Position.DistanceToSquared(weapon.Position);
                if (dist < bestDist) { bestDist = dist; hauler = p; }
            }
        }

        if (hauler == null)
        {
            Messages.Message("FCP_RepairBench_NoHauler".Translate(), MessageTypeDefOf.RejectInput, historical: false);
            return;
        }

        hauler.jobs.StartJob(
            JobMaker.MakeJob(WeaponConditionDefOf.FCP_Job_LoadWeaponToBench, parent, weapon),
            JobCondition.InterruptForced);
    }

    private void Eject()
    {
        parent.Map.dynamicDrawManager.DeRegisterDrawable(parent);
        slot.TryDropAll(parent.InteractionCell, parent.Map, ThingPlaceMode.Near);
    }
}
