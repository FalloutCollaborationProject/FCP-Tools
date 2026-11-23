using UnityEngine;

namespace FCP.Synths;

public enum SynthPodState
{
    Idle,
    Gestating,
    UpdatingProgramming
}

public class Building_SynthesizerPod : Building, IThingHolder
{
    private const float IngredientSearchRadius = 8f;

    private SynthPodState state = SynthPodState.Idle;
    private SynthTierDef activeTier;
    private int ticksRemaining;
    private int programmingTicksRemaining;

    private ThingOwner<Pawn> innerContainer;
    private CompPowerTrader powerComp;

    public Building_SynthesizerPod()
    {
        innerContainer = new ThingOwner<Pawn>(this, oneStackOnly: true);
    }

    private Pawn ContainedPawn => innerContainer.Count > 0 ? innerContainer[0] : null;

    public ThingOwner GetDirectlyHeldThings()
    {
        return innerContainer;
    }

    public void GetChildHolders(List<IThingHolder> outChildren)
    {
        ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        powerComp = GetComp<CompPowerTrader>();
        UpdatePowerDraw();
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref state, "synthPodState", SynthPodState.Idle);
        Scribe_Defs.Look(ref activeTier, "activeTier");
        Scribe_Values.Look(ref ticksRemaining, "ticksRemaining", 0);
        Scribe_Values.Look(ref programmingTicksRemaining, "programmingTicksRemaining", 0);
        Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
    }

    protected override void Tick()
    {
        base.Tick();

        if (state == SynthPodState.Idle)
            return;

        if (powerComp != null && !powerComp.PowerOn)
            return;

        if (state == SynthPodState.Gestating)
        {
            ticksRemaining--;
            if (ticksRemaining <= 0)
                FinalizeGestation();
            return;
        }

        programmingTicksRemaining--;
        if (programmingTicksRemaining <= 0)
            FinalizeProgramming();
    }

    public override void DynamicDrawPhaseAt(DrawPhase phase, Vector3 drawLoc, bool flip = false)
    {
        ContainedPawn?.Drawer.renderer.DynamicDrawPhaseAt(phase, drawLoc, null, neverAimWeapon: true);
        base.DynamicDrawPhaseAt(phase, drawLoc, flip);
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (Gizmo gizmo in base.GetGizmos())
            yield return gizmo;

        if (state == SynthPodState.Idle)
        {
            yield return new Command_Action
            {
                defaultLabel = "FCP_SelectSynthTier".Translate(),
                defaultDesc = "FCP_SelectSynthTierDesc".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Commands/ReleaseAnimals"),
                action = OpenTierMenu
            };
        }

        if (Prefs.DevMode)
        {
            yield return new Command_Action
            {
                defaultLabel = "DEV: Instant synth",
                defaultDesc = "Skips research, materials, and gestation time. Spawns immediately.",
                action = OpenDevInstantTierMenu
            };
        }
    }

    public override string GetInspectString()
    {
        string text = base.GetInspectString();

        if (state == SynthPodState.Gestating && activeTier != null)
        {
            float totalTicks = activeTier.gestationDays * GenDate.TicksPerDay;
            float pct = 1f - ticksRemaining / totalTicks;

            if (!text.NullOrEmpty())
                text += "\n";

            text += "FCP_SynthPodGestating".Translate(activeTier.LabelCap, pct.ToStringPercent());
        }
        else if (state == SynthPodState.UpdatingProgramming)
        {
            float pct = 1f - programmingTicksRemaining / (float)GenDate.TicksPerDay;

            if (!text.NullOrEmpty())
                text += "\n";

            text += "FCP_SynthPodUpdatingProgramming".Translate(pct.ToStringPercent());
        }

        if (state != SynthPodState.Idle && powerComp != null && !powerComp.PowerOn)
            text += "\n" + "FCP_SynthPodNoPower".Translate();

        return text;
    }

    private void OpenTierMenu()
    {
        List<Thing> nearby = GetNearbyThings();
        List<FloatMenuOption> options = new();

        foreach (SynthTierDef tier in DefDatabase<SynthTierDef>.AllDefsListForReading.OrderBy(t => t.tier))
        {
            if (tier.requiredResearch != null && !tier.requiredResearch.IsFinished)
                continue;

            SynthTierDef capturedTier = tier;
            string cost = FormatIngredients(capturedTier);

            if (!HasIngredients(capturedTier, nearby))
            {
                options.Add(new FloatMenuOption($"{capturedTier.LabelCap} ({cost}) - {"FCP_SynthPodMissingMaterials".Translate()}", null));
                continue;
            }

            options.Add(new FloatMenuOption($"{capturedTier.LabelCap} ({cost})", () => BeginGestation(capturedTier)));
        }

        if (options.Count == 0)
        {
            Messages.Message("FCP_SynthPodNothingResearched".Translate(), MessageTypeDefOf.RejectInput, false);
            return;
        }

        Find.WindowStack.Add(new FloatMenu(options));
    }

    private void OpenDevInstantTierMenu()
    {
        List<FloatMenuOption> options = new();

        foreach (SynthTierDef tier in DefDatabase<SynthTierDef>.AllDefsListForReading.OrderBy(t => t.tier))
        {
            SynthTierDef capturedTier = tier;
            options.Add(new FloatMenuOption(capturedTier.LabelCap, () => CompleteSynth(GenerateSynthPawn(capturedTier))));
        }

        Find.WindowStack.Add(new FloatMenu(options));
    }

    private static string FormatIngredients(SynthTierDef tier)
    {
        return string.Join(", ", tier.ingredients.Select(i => $"{i.thingDef.LabelCap} x{i.count}"));
    }

    private List<Thing> GetNearbyThings()
    {
        return GenRadial.RadialDistinctThingsAround(Position, Map, IngredientSearchRadius, true)
            .Where(t => t.Spawned)
            .ToList();
    }

    private static bool HasIngredients(SynthTierDef tier, List<Thing> nearby)
    {
        foreach (ThingDefCountClass need in tier.ingredients)
        {
            int available = nearby.Where(t => t.def == need.thingDef).Sum(t => t.stackCount);

            if (available < need.count)
                return false;
        }

        return true;
    }

    private bool TryConsumeIngredients(SynthTierDef tier)
    {
        List<Thing> nearby = GetNearbyThings();
        List<(Thing thing, int amount)> toConsume = new();

        foreach (ThingDefCountClass need in tier.ingredients)
        {
            int remaining = need.count;

            foreach (Thing thing in nearby)
            {
                if (remaining <= 0)
                    break;

                if (thing.def != need.thingDef)
                    continue;

                int take = Mathf.Min(remaining, thing.stackCount);
                toConsume.Add((thing, take));
                remaining -= take;
            }

            if (remaining > 0)
                return false;
        }

        foreach ((Thing thing, int amount) in toConsume)
            thing.SplitOff(amount)?.Destroy();

        return true;
    }

    private void BeginGestation(SynthTierDef tier)
    {
        if (!TryConsumeIngredients(tier))
        {
            Messages.Message("FCP_SynthPodMissingMaterials".Translate(), MessageTypeDefOf.RejectInput, false);
            return;
        }

        innerContainer.TryAdd(GenerateSynthPawn(tier));

        activeTier = tier;
        ticksRemaining = Mathf.RoundToInt(tier.gestationDays * GenDate.TicksPerDay);
        state = SynthPodState.Gestating;
        UpdatePowerDraw();
    }

    private void FinalizeGestation()
    {
        ticksRemaining = 0;

        if (activeTier.tier == 3)
        {
            state = SynthPodState.UpdatingProgramming;
            programmingTicksRemaining = GenDate.TicksPerDay;
            UpdatePowerDraw();

            if (ContainedPawn != null)
                Messages.Message("FCP_SynthPodUpdatingProgramming_Start".Translate(ContainedPawn.LabelShortCap), this, MessageTypeDefOf.NeutralEvent);

            return;
        }

        CompleteSynth(ContainedPawn);
    }

    private void FinalizeProgramming()
    {
        programmingTicksRemaining = 0;
        CompleteSynth(ContainedPawn);
    }

    private void CompleteSynth(Pawn pawn)
    {
        activeTier = null;
        state = SynthPodState.Idle;
        UpdatePowerDraw();

        if (pawn == null)
            return;

        innerContainer.Remove(pawn);
        GenSpawn.Spawn(pawn, InteractionCell, Map);
        pawn.apparel.DestroyAll(DestroyMode.Vanish);
        pawn.mindState.nextApparelOptimizeTick = Find.TickManager.TicksGame + 60000;
        pawn.Drawer.renderer.SetAllGraphicsDirty();

        Messages.Message("FCP_SynthPodComplete".Translate(pawn.LabelShortCap), pawn, MessageTypeDefOf.PositiveEvent);
        Find.WindowStack.Add(new Window_SynthCustomization(pawn));
    }

    private void UpdatePowerDraw()
    {
        if (powerComp == null)
            return;

        float watts = state == SynthPodState.Gestating
            ? activeTier?.powerDrawActiveWatts ?? 50f
            : activeTier?.powerDrawIdleWatts ?? 50f;

        powerComp.PowerOutput = -watts;
    }

    private static Pawn GenerateSynthPawn(SynthTierDef tier)
    {
        PawnGenerationRequest request = new(tier.pawnKindDef, Faction.OfPlayer, PawnGenerationContext.NonPlayer, forceGenerateNewPawn: true);
        Pawn pawn = PawnGenerator.GeneratePawn(request);
        pawn.apparel.DestroyAll(DestroyMode.Vanish);

        if (tier.bodyOverrideHediff != null && !pawn.health.hediffSet.HasHediff(tier.bodyOverrideHediff))
            pawn.health.AddHediff(tier.bodyOverrideHediff);

        if (tier.yieldHediff != null && tier.yieldHediff != tier.bodyOverrideHediff && !pawn.health.hediffSet.HasHediff(tier.yieldHediff))
            pawn.health.AddHediff(tier.yieldHediff);

        if (tier.tier == 3)
        {
            pawn.story.bodyType = BodyTypeDefOf.Male;
            pawn.story.hairDef = HairDefOf.Bald;
            pawn.story.skinColorOverride = Color.white;
        }

        return pawn;
    }
}
