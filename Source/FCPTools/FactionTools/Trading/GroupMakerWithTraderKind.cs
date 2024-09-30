// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable ClassNeverInstantiated.Global

using System.Collections.Generic;
using RimWorld;

namespace FCP.Factions;

public class GroupMakerWithTraderKind : PawnGroupMaker
{
    public List<TraderKindDef> traderKinds = [];
}