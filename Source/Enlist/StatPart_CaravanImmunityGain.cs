using RimWorld;
using RimWorld.Planet;
using Verse;

namespace  FCP.Enlist
{
    public class StatPart_CaravanImmunityGain : StatPart
    {
        public override void TransformValue(StatRequest req, ref float val)
        {
            if (req.Thing is Pawn pawn)
            {
                var caravan = pawn.GetCaravan();
                if (caravan != null && !caravan.pather.moving && !caravan.NightResting)
                {
                    var settlement = Find.WorldObjects.SettlementAt(caravan.Tile);
                    if (settlement != null)
                    {
                        foreach (var def in settlement.Faction.GetEnlistOptions())
                        {
                            if (WorldEnlistTracker.Instance.EnlistedTo(settlement.Faction, def))
                            {
                                var comp = settlement.GetComponent<WorldObjectCompEnlist>();
                                var caravanOptions = comp.GetCaravanOptions(caravan);
                                if (caravanOptions?.curWorkOption != null && caravanOptions.curWorkOption.immunityGainSpeedMultiplier.HasValue)
                                {
                                    val *= caravanOptions.curWorkOption.immunityGainSpeedMultiplier.Value;
                                }
                            }
                        }
                    }
                }
            }
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (req.Thing is Pawn pawn)
            {
                var caravan = pawn.GetCaravan();
                if (caravan != null && !caravan.pather.moving && !caravan.NightResting)
                {
                    var settlement = Find.WorldObjects.SettlementAt(caravan.Tile);
                    if (settlement != null)
                    {
                        var comp = settlement.GetComponent<WorldObjectCompEnlist>();
                        var caravanOptions = comp.GetCaravanOptions(caravan);
                        if (caravanOptions?.curWorkOption != null && caravanOptions.curWorkOption.immunityGainSpeedMultiplier.HasValue)
                        {
                            return $"Immunity gain speed multiplier: x{caravanOptions.curWorkOption.immunityGainSpeedMultiplier.Value:0.##}";
                        }
                    }
                }
            }
            return null;
        }
    }
}
