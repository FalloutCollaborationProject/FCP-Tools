namespace FCP.Core;

public class Comp_SyringerAmmo : ThingComp
{
    private ThingDef loadedSyringeDef;

    public ThingDef LoadedSyringeDef => loadedSyringeDef;

    private Pawn WieldingPawn => (parent.ParentHolder as Pawn_EquipmentTracker)?.pawn;

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Defs.Look(ref loadedSyringeDef, "loadedSyringeDef");
    }

    public void Notify_ShotFired()
    {
        loadedSyringeDef = null;
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        foreach (Gizmo gizmo in base.CompGetGizmosExtra())
        {
            yield return gizmo;
        }

        Pawn pawn = WieldingPawn;
        if (pawn == null || !pawn.IsColonistPlayerControlled)
        {
            yield break;
        }

        yield return new Command_Action
        {
            defaultLabel = loadedSyringeDef == null
                ? "FCP_Syringer_Load".Translate()
                : "FCP_Syringer_Reload".Translate(loadedSyringeDef.LabelCap),
            defaultDesc = "FCP_Syringer_Load_Desc".Translate(),
            icon = parent.def.uiIcon,
            action = delegate { OpenLoadMenu(pawn); }
        };
    }

    private void OpenLoadMenu(Pawn pawn)
    {
        List<FloatMenuOption> options = new List<FloatMenuOption>();
        IEnumerable<ThingDef> syringeDefs = pawn.inventory?.innerContainer
            .Where(t => t.def.GetModExtension<DefExtension_SyringeAmmo>() != null)
            .Select(t => t.def)
            .Distinct();

        if (syringeDefs != null)
        {
            foreach (ThingDef syringeDef in syringeDefs)
            {
                ThingDef defLocal = syringeDef;
                options.Add(new FloatMenuOption(defLocal.LabelCap, delegate
                {
                    Thing syringeThing = pawn.inventory.innerContainer.FirstOrDefault(t => t.def == defLocal);
                    syringeThing?.SplitOff(1).Destroy();
                    loadedSyringeDef = defLocal;
                }));
            }
        }

        if (options.Count == 0)
        {
            options.Add(new FloatMenuOption("FCP_Syringer_NoSyringes".Translate(), null));
        }

        Find.WindowStack.Add(new FloatMenu(options));
    }
}
