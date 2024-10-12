using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;

namespace FCP.Core;

[UsedImplicitly]
[SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
public class StoryTellerIsJoinerExtension : DefModExtension
{
    // Forced Defs
    public PawnKindDef pawnKindDef;
    public XenotypeDef forcedXenotypeDef;

    // Misc
    public FactionDef originalFactionDef;
    public RoyalTitleDef originalFactionTitle;
    public bool useOriginalFactionIdeo = false;
    public bool onlyFixedPawnKindBackstories = false;

    // Appearance
    public HairDef hairDef;
    public BeardDef beardDef;
    public TattooDef tattooHead;
    public TattooDef tattooBody;
    public Color? hairColor;
    public Color? skinColorOverride;
    
    // Characteristics
    public string firstName;
    public string lastName;
    public string nickname;

    public Gender? gender = null;
    public float? biologicalAge = null;
    public float? chronologicalAge = null;

    public override IEnumerable<string> ConfigErrors()
    {
        if (originalFactionDef == null && (originalFactionTitle != null || useOriginalFactionIdeo))
            yield return "originalFactionTitle or useOriginalFactionIdeo are set, but originalFactionDef isn't";
    }
}

