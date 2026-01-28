using System.Collections.Generic;
using Verse;

namespace FCP.PocketMaps
{
    public class ModExtensionPresettablePocketMap : DefModExtension
    {
        public List<string> prefabDefs;
        public string abandonedMessage = "This pocket map has been abandoned and can no longer be entered.";
    }
}
