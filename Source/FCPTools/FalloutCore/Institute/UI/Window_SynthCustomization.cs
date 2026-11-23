using UnityEngine;
using FCP.Core;
using FCP.Core.Hediffs;

namespace FCP.Synths;

public class Window_SynthCustomization : Window
{
    private const float PortraitSize = 220f;
    private const float IconSize = 40f;
    private const float RowHeight = 30f;
    private const int MinAge = 3;
    private const int MaxAge = 100;

    private readonly Pawn pawn;

    private string firstName;
    private string nickName;
    private string lastName;

    private readonly int initialAge;
    private string ageInput;

    private readonly List<BackstoryDef> availableChildhoodBackstories;
    private BackstoryDef selectedChildhoodBackstory;
    private readonly List<BackstoryDef> availableAdulthoodBackstories;
    private BackstoryDef selectedAdulthoodBackstory;

    private List<HairDef> availableHair;
    private readonly List<BeardDef> availableBeards;
    private readonly List<TattooDef> availableFaceTattoos;
    private readonly List<TattooDef> availableBodyTattoos;

    private static readonly Dictionary<StyleItemDef, Texture2D> StyleIconCache = new();

    private static readonly List<Color> SkinTonePalette = new()
    {
        new Color(242f / 255f, 237f / 255f, 224f / 255f),
        new Color(255f / 255f, 239f / 255f, 213f / 255f),
        new Color(255f / 255f, 239f / 255f, 201f / 255f),
        new Color(255f / 255f, 239f / 255f, 189f / 255f),
        new Color(249f / 255f, 219f / 255f, 165f / 255f),
        new Color(242f / 255f, 199f / 255f, 140f / 255f),
        new Color(228f / 255f, 158f / 255f, 90f / 255f),
        new Color(130f / 255f, 91f / 255f, 48f / 255f),
        new Color(99f / 255f, 70f / 255f, 36f / 255f),
    };

    private static readonly List<Color> HairColorPalette = new()
    {
        PawnHairColors.DarkBlack,
        PawnHairColors.MidBlack,
        PawnHairColors.DarkBrown,
        PawnHairColors.ReddishBrown,
        PawnHairColors.DarkReddish,
        PawnHairColors.DarkSaturatedReddish,
        PawnHairColors.SandyBlonde,
        PawnHairColors.Blonde,
        new Color(0.85f, 0.85f, 0.85f),
        new Color(0.75f, 0.75f, 0.75f),
        new Color(0.65f, 0.65f, 0.65f),
        new Color(0.8f, 0.1f, 0.1f),
        new Color(0.9f, 0.45f, 0.05f),
        new Color(0.85f, 0.8f, 0.1f),
        new Color(0.15f, 0.7f, 0.25f),
        new Color(0.1f, 0.75f, 0.75f),
        new Color(0.15f, 0.35f, 0.85f),
        new Color(0.5f, 0.15f, 0.8f),
        new Color(0.9f, 0.25f, 0.6f),
    };

    private static readonly List<(BodyTypeDef def, string labelKey)> AdultBodyTypes = new()
    {
        (BodyTypeDefOf.Male, "FCP_BodyType_Male"),
        (BodyTypeDefOf.Female, "FCP_BodyType_Female"),
        (BodyTypeDefOf.Thin, "FCP_BodyType_Thin"),
        (BodyTypeDefOf.Fat, "FCP_BodyType_Fat"),
        (BodyTypeDefOf.Hulk, "FCP_BodyType_Hulk"),
    };

    private readonly bool bodyTypeMatters;

    private Vector2 scrollPosition;

    private static Color DimColor => new(TerminalColors.PrimaryColor.r, TerminalColors.PrimaryColor.g, TerminalColors.PrimaryColor.b, 0.5f);

    public override Vector2 InitialSize => new(760f, 640f);

    public Window_SynthCustomization(Pawn pawn)
    {
        this.pawn = pawn;
        doCloseX = false;
        closeOnCancel = false;
        forcePause = true;
        absorbInputAroundWindow = true;

        NameTriple name = pawn.Name as NameTriple;
        firstName = name?.First ?? pawn.LabelShort;
        nickName = name?.Nick ?? string.Empty;
        lastName = name?.Last ?? string.Empty;

        initialAge = pawn.ageTracker.AgeBiologicalYears;
        ageInput = initialAge.ToString();

        availableChildhoodBackstories = GetAvailableBackstories(BackstorySlot.Childhood);
        selectedChildhoodBackstory = pawn.story.Childhood ?? availableChildhoodBackstories.FirstOrDefault();

        availableAdulthoodBackstories = GetAvailableBackstories(BackstorySlot.Adulthood);
        selectedAdulthoodBackstory = pawn.story.Adulthood ?? availableAdulthoodBackstories.FirstOrDefault();

        availableHair = ComputeAvailableHair();
        availableBeards = DefDatabase<BeardDef>.AllDefsListForReading.ToList();

        bodyTypeMatters = pawn.health.hediffSet.hediffs
            .OfType<HediffWithComps>()
            .All(h => h.TryGetComp<HediffComp_BodyGraphicOverride>() == null);

        if (ModsConfig.IdeologyActive)
        {
            availableFaceTattoos = DefDatabase<TattooDef>.AllDefsListForReading.Where(t => t.tattooType == TattooType.Face).ToList();
            availableBodyTattoos = DefDatabase<TattooDef>.AllDefsListForReading.Where(t => t.tattooType == TattooType.Body).ToList();
        }
    }

    private List<HairDef> ComputeAvailableHair()
    {
        return DefDatabase<HairDef>.AllDefsListForReading
            .Where(h => PawnStyleItemChooser.AgeAppropriateHairStyle(pawn, h) && GenderMatches(h.styleGender, pawn.gender))
            .ToList();
    }

    private static bool GenderMatches(StyleGender styleGender, Gender gender)
    {
        return styleGender switch
        {
            StyleGender.Any => true,
            StyleGender.Male or StyleGender.MaleUsually => gender == Gender.Male,
            StyleGender.Female or StyleGender.FemaleUsually => gender == Gender.Female,
            _ => true
        };
    }

    private List<BackstoryDef> GetAvailableBackstories(BackstorySlot slot)
    {
        BackstoryCategoryFilter filter = pawn.kindDef.backstoryFiltersOverride?.FirstOrDefault();
        List<string> categories = slot == BackstorySlot.Childhood ? filter?.categoriesChildhood : filter?.categoriesAdulthood;

        if (categories.NullOrEmpty())
            return DefDatabase<BackstoryDef>.AllDefsListForReading.Where(b => b.slot == slot).ToList();

        return DefDatabase<BackstoryDef>.AllDefsListForReading
            .Where(b => b.slot == slot && b.spawnCategories.Any(c => categories.Contains(c)))
            .ToList();
    }

    public override void DoWindowContents(Rect inRect)
    {
        Text.Font = GameFont.Medium;
        GUI.color = TerminalColors.PrimaryColor;
        Widgets.Label(new Rect(0f, 0f, inRect.width, 35f), "FCP_SynthCustomizeTitle".Translate());
        GUI.color = Color.white;
        Text.Font = GameFont.Small;

        Rect portraitRect = new(0f, 45f, PortraitSize, PortraitSize);
        DrawPortrait(portraitRect);

        Rect bodyRect = new(PortraitSize + 20f, 45f, inRect.width - PortraitSize - 20f, inRect.height - 90f);
        DrawBody(bodyRect);

        GUI.color = TerminalColors.PrimaryColor;
        Rect finalizeRect = new(inRect.width - 160f, inRect.height - 40f, 160f, 35f);
        if (Widgets.ButtonText(finalizeRect, "FCP_SynthCustomizeFinalize".Translate()))
            ConfirmAndClose();
        GUI.color = Color.white;
    }

    private void DrawPortrait(Rect rect)
    {
        GUI.color = TerminalColors.PrimaryColor;
        Widgets.DrawBox(rect);
        GUI.color = Color.white;

        RenderTexture portrait = PortraitsCache.Get(pawn, new Vector2(rect.width - 4f, rect.height - 4f), Rot4.South,
            Vector3.zero, 1f, true, true, true, true, null, null, false, null);

        GUI.DrawTexture(rect.ContractedBy(2f), portrait);
    }

    private void DrawBody(Rect rect)
    {
        float contentWidth = rect.width - 20f;
        float totalHeight = MeasureContentHeight(contentWidth);

        Rect viewRect = new(0f, 0f, contentWidth, totalHeight);
        GUI.color = TerminalColors.PrimaryColor;
        Widgets.BeginScrollView(rect, ref scrollPosition, viewRect);
        GUI.color = Color.white;

        float y = 0f;
        DrawNameSection(ref y, contentWidth);
        DrawAgeSection(ref y, contentWidth);

        DrawColorSection(ref y, contentWidth, "FCP_SynthCustomizeSkin".Translate(), pawn.story.SkinColor,
            SkinTonePalette, color => pawn.story.skinColorOverride = color);
        DrawColorSection(ref y, contentWidth, "FCP_SynthCustomizeHairColor".Translate(), pawn.story.HairColor,
            HairColorPalette, color => pawn.story.HairColor = color);

        if (bodyTypeMatters)
            DrawBodyTypeSection(ref y, contentWidth);

        DrawStyleItemSection(ref y, contentWidth, "FCP_SynthCustomizeHair".Translate(), availableHair,
            pawn.story.hairDef, h => pawn.story.hairDef = h);

        if (pawn.style.CanWantBeard)
            DrawStyleItemSection(ref y, contentWidth, "FCP_SynthCustomizeBeard".Translate(), availableBeards,
                pawn.style.beardDef, b => pawn.style.beardDef = b);

        if (ModsConfig.IdeologyActive)
        {
            DrawStyleItemSection(ref y, contentWidth, "FCP_SynthCustomizeFaceTattoo".Translate(), availableFaceTattoos,
                pawn.style.FaceTattoo, t => pawn.style.FaceTattoo = t);
            DrawStyleItemSection(ref y, contentWidth, "FCP_SynthCustomizeBodyTattoo".Translate(), availableBodyTattoos,
                pawn.style.BodyTattoo, t => pawn.style.BodyTattoo = t);
        }

        DrawBackstorySection(ref y, contentWidth, "FCP_SynthCustomizeChildhoodBackstory".Translate(), availableChildhoodBackstories,
            selectedChildhoodBackstory, b =>
            {
                selectedChildhoodBackstory = b;
                pawn.story.Childhood = b;
            });

        DrawBackstorySection(ref y, contentWidth, "FCP_SynthCustomizeBackstory".Translate(), availableAdulthoodBackstories,
            selectedAdulthoodBackstory, b =>
            {
                selectedAdulthoodBackstory = b;
                pawn.story.Adulthood = b;
            });

        Widgets.EndScrollView();
    }

    private float MeasureContentHeight(float width)
    {
        float height = 0f;
        height += RowHeight * 3f + 10f;
        height += RowHeight + 10f;
        height += RowHeight + IconRowHeight(SkinTonePalette.Count, width) + 15f;
        height += RowHeight + IconRowHeight(HairColorPalette.Count, width) + 15f;

        if (bodyTypeMatters)
            height += RowHeight * 2f + 10f;

        height += RowHeight + IconRowHeight(availableHair.Count, width) + 15f;

        if (pawn.style.CanWantBeard)
            height += RowHeight + IconRowHeight(availableBeards.Count, width) + 15f;

        if (ModsConfig.IdeologyActive)
        {
            height += RowHeight + IconRowHeight(availableFaceTattoos.Count, width) + 15f;
            height += RowHeight + IconRowHeight(availableBodyTattoos.Count, width) + 15f;
        }

        height += RowHeight * (availableChildhoodBackstories.Count + 1) + 10f;
        height += RowHeight * (availableAdulthoodBackstories.Count + 1) + 10f;
        return height;
    }

    private static void DrawSectionLabel(Rect rect, string label)
    {
        Text.Font = GameFont.Tiny;
        GUI.color = DimColor;
        Widgets.Label(rect, label);
        GUI.color = Color.white;
        Text.Font = GameFont.Small;
    }

    private static float IconRowHeight(int itemCount, float width)
    {
        int perRow = Mathf.Max(1, Mathf.FloorToInt(width / (IconSize + 6f)));
        int rows = Mathf.CeilToInt(itemCount / (float)perRow);
        return rows * (IconSize + 6f);
    }

    private void DrawNameSection(ref float y, float width)
    {
        DrawSectionLabel(new Rect(0f, y, width, RowHeight), "FCP_SynthCustomizeName".Translate());
        DrawGenderToggle(y, width);
        y += RowHeight;

        float fieldWidth = width / 3f - 6f;
        firstName = Widgets.TextField(new Rect(0f, y, fieldWidth, RowHeight), firstName);
        nickName = Widgets.TextField(new Rect(fieldWidth + 9f, y, fieldWidth, RowHeight), nickName);
        lastName = Widgets.TextField(new Rect((fieldWidth + 9f) * 2f, y, fieldWidth, RowHeight), lastName);
        y += RowHeight * 2f + 10f;
    }

    private void DrawGenderToggle(float y, float width)
    {
        const float buttonWidth = 70f;

        Rect femaleRect = new(width - buttonWidth, y, buttonWidth, RowHeight);
        Rect maleRect = new(femaleRect.x - buttonWidth - 6f, y, buttonWidth, RowHeight);

        DrawGenderButton(maleRect, Gender.Male);
        DrawGenderButton(femaleRect, Gender.Female);
    }

    private void DrawGenderButton(Rect rect, Gender gender)
    {
        GUI.color = TerminalColors.PrimaryColor;
        if (Widgets.ButtonText(rect, gender.GetLabel()))
            SetGender(gender);
        GUI.color = Color.white;

        if (pawn.gender == gender)
        {
            GUI.color = TerminalColors.HighlightColor;
            Widgets.DrawBox(rect, 2);
            GUI.color = Color.white;
        }
    }

    private void SetGender(Gender gender)
    {
        if (pawn.gender == gender)
            return;

        pawn.gender = gender;

        Gene headGene = pawn.genes?.GenesListForReading.FirstOrDefault(g => g.Active && !g.def.forcedHeadTypes.NullOrEmpty());
        if (headGene == null || !pawn.story.TryGetRandomHeadFromSet(headGene.def.forcedHeadTypes))
            pawn.story.TryGetRandomHeadFromSet(DefDatabase<HeadTypeDef>.AllDefs.Where(h => h.randomChosen));

        pawn.story.hairDef = PawnStyleItemChooser.RandomHairFor(pawn);
        availableHair = ComputeAvailableHair();
        pawn.style.beardDef = PawnStyleItemChooser.RandomBeardFor(pawn);

        if (pawn.story.bodyType == OppositeGenderBodyType(gender))
            pawn.story.bodyType = gender == Gender.Male ? BodyTypeDefOf.Male : BodyTypeDefOf.Female;

        pawn.Drawer.renderer.SetAllGraphicsDirty();
    }

    private void DrawAgeSection(ref float y, float width)
    {
        DrawSectionLabel(new Rect(0f, y, width, RowHeight), "FCP_SynthCustomizeAge".Translate());

        const float buttonWidth = 30f;
        const float fieldWidth = 60f;

        Rect plusRect = new(width - buttonWidth, y, buttonWidth, RowHeight);
        Rect fieldRect = new(plusRect.x - fieldWidth - 4f, y, fieldWidth, RowHeight);
        Rect minusRect = new(fieldRect.x - buttonWidth - 4f, y, buttonWidth, RowHeight);

        GUI.color = TerminalColors.PrimaryColor;
        if (Widgets.ButtonText(minusRect, "-"))
            SetAge(pawn.ageTracker.AgeBiologicalYears - 1);

        ageInput = Widgets.TextField(fieldRect, ageInput);

        if (Widgets.ButtonText(plusRect, "+"))
            SetAge(pawn.ageTracker.AgeBiologicalYears + 1);
        GUI.color = Color.white;

        if (int.TryParse(ageInput, out int parsed) && parsed != pawn.ageTracker.AgeBiologicalYears)
            SetAge(parsed);

        y += RowHeight + 10f;
    }

    private void SetAge(int years)
    {
        years = Mathf.Clamp(years, MinAge, MaxAge);
        if (years == pawn.ageTracker.AgeBiologicalYears)
            return;

        pawn.ageTracker.AgeBiologicalTicks = years * 3600000L;
        pawn.ageTracker.AgeChronologicalTicks = pawn.ageTracker.AgeBiologicalTicks;
        ageInput = years.ToString();

        pawn.Drawer.renderer.SetAllGraphicsDirty();
    }

    private void DrawColorSection(ref float y, float width, string label, Color current, List<Color> palette, Action<Color> onPick)
    {
        DrawSectionLabel(new Rect(0f, y, width, RowHeight), label);
        y += RowHeight;

        float rowHeight = IconRowHeight(palette.Count, width);
        float x = 0f;
        float rowY = y;

        foreach (Color color in palette)
        {
            if (x + IconSize > width)
            {
                x = 0f;
                rowY += IconSize + 6f;
            }

            Rect swatchRect = new(x, rowY, IconSize, IconSize);
            Widgets.DrawBoxSolid(swatchRect, color);

            bool selected = color == current;
            GUI.color = selected ? TerminalColors.HighlightColor : TerminalColors.PrimaryColor;
            Widgets.DrawBox(swatchRect, selected ? 3 : 1);
            GUI.color = Color.white;

            if (Widgets.ButtonInvisible(swatchRect))
            {
                onPick(color);
                pawn.Drawer.renderer.SetAllGraphicsDirty();
            }

            x += IconSize + 6f;
        }

        y += rowHeight + 15f;
    }

    private List<(BodyTypeDef def, string labelKey)> AvailableBodyTypes =>
        AdultBodyTypes.Where(bt => bt.def != OppositeGenderBodyType(pawn.gender)).ToList();

    private static BodyTypeDef OppositeGenderBodyType(Gender gender)
    {
        return gender switch
        {
            Gender.Male => BodyTypeDefOf.Female,
            Gender.Female => BodyTypeDefOf.Male,
            _ => null
        };
    }

    private void DrawBodyTypeSection(ref float y, float width)
    {
        DrawSectionLabel(new Rect(0f, y, width, RowHeight), "FCP_SynthCustomizeBodyType".Translate());
        y += RowHeight;

        List<(BodyTypeDef def, string labelKey)> options = AvailableBodyTypes;
        float spacing = 6f;
        float buttonWidth = (width - spacing * (options.Count - 1)) / options.Count;
        float x = 0f;

        foreach ((BodyTypeDef bodyType, string labelKey) in options)
        {
            Rect buttonRect = new(x, y, buttonWidth, RowHeight);

            GUI.color = TerminalColors.PrimaryColor;
            if (Widgets.ButtonText(buttonRect, labelKey.Translate()))
            {
                pawn.story.bodyType = bodyType;
                pawn.Drawer.renderer.SetAllGraphicsDirty();
            }
            GUI.color = Color.white;

            if (bodyType == pawn.story.bodyType)
            {
                GUI.color = TerminalColors.HighlightColor;
                Widgets.DrawBox(buttonRect, 2);
                GUI.color = Color.white;
            }

            x += buttonWidth + spacing;
        }

        y += RowHeight + 10f;
    }

    private void DrawStyleItemSection<T>(ref float y, float width, string label, List<T> options, T current,
        Action<T> onPick) where T : StyleItemDef
    {
        DrawSectionLabel(new Rect(0f, y, width, RowHeight), label);
        y += RowHeight;

        float rowHeight = IconRowHeight(options.Count, width);
        float x = 0f;
        float rowY = y;

        foreach (T option in options)
        {
            if (x + IconSize > width)
            {
                x = 0f;
                rowY += IconSize + 6f;
            }

            Rect iconRect = new(x, rowY, IconSize, IconSize);

            if (Widgets.ButtonImage(iconRect, ResolveStyleIcon(option)))
            {
                onPick(option);
                pawn.Drawer.renderer.SetAllGraphicsDirty();
            }

            if (option == current)
            {
                GUI.color = TerminalColors.HighlightColor;
                Widgets.DrawBox(iconRect, 3);
                GUI.color = Color.white;
            }

            TooltipHandler.TipRegion(iconRect, option.LabelCap);

            x += IconSize + 6f;
        }

        y += rowHeight + 15f;
    }

    private static Texture2D ResolveStyleIcon(StyleItemDef styleItem)
    {
        if (StyleIconCache.TryGetValue(styleItem, out Texture2D cached))
            return cached;

        if (styleItem.iconPath.NullOrEmpty() && styleItem.texPath.NullOrEmpty())
        {
            StyleIconCache[styleItem] = BaseContent.ClearTex;
            return BaseContent.ClearTex;
        }

        Texture2D icon = null;

        if (!styleItem.iconPath.NullOrEmpty())
            icon = ContentFinder<Texture2D>.Get(styleItem.iconPath, false);

        if (icon == null && !styleItem.texPath.NullOrEmpty())
            icon = ContentFinder<Texture2D>.Get(styleItem.texPath + "_south", false) ?? ContentFinder<Texture2D>.Get(styleItem.texPath, false);

        icon ??= BaseContent.BadTex;
        StyleIconCache[styleItem] = icon;
        return icon;
    }

    private void DrawBackstorySection(ref float y, float width, string label, List<BackstoryDef> options, BackstoryDef current, Action<BackstoryDef> onPick)
    {
        DrawSectionLabel(new Rect(0f, y, width, RowHeight), label);
        y += RowHeight;

        foreach (BackstoryDef backstory in options)
        {
            Rect rowRect = new(0f, y, width, RowHeight);

            if (backstory == current)
                Widgets.DrawHighlightSelected(rowRect);
            else
                Widgets.DrawLightHighlight(rowRect);

            if (Widgets.ButtonInvisible(rowRect))
                onPick(backstory);

            Widgets.Label(new Rect(rowRect.x + 6f, rowRect.y, rowRect.width - 12f, rowRect.height), backstory.title.CapitalizeFirst().StripTags());
            TooltipHandler.TipRegion(rowRect, () => ((string)backstory.FullDescriptionFor(pawn)).StripTags(), backstory.GetHashCode());
            y += RowHeight;
        }
    }

    private void ConfirmAndClose()
    {
        if (pawn.Name is NameTriple)
        {
            pawn.Name = new NameTriple(
                firstName.NullOrEmpty() ? pawn.LabelShort : firstName,
                nickName,
                lastName);
        }

        if (pawn.ageTracker.AgeBiologicalYears != initialAge)
            AgeInjuryUtility.GenerateRandomOldAgeInjuries(pawn, tryNotToKillPawn: true);

        pawn.Drawer.renderer.SetAllGraphicsDirty();
        Close();
    }
}
