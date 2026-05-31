using System.Collections.Generic;
using Verse;

namespace FCP.Core;

public class CompUsableOnce : CompUsable
{
    private CompUseEffect_ItemBox cachedItemBox;

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        if (cachedItemBox == null)
        {
            cachedItemBox = parent.GetComp<CompUseEffect_ItemBox>();
        }

        if (cachedItemBox != null && cachedItemBox.LootClaimed)
        {
            yield break;
        }

        foreach (Gizmo gizmo in base.CompGetGizmosExtra())
        {
            yield return gizmo;
        }
    }
}
