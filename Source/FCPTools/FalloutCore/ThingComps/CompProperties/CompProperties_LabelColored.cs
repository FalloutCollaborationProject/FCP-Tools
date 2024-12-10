using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCP.Core
{
    public class CompProperties_LabelColored : CompProperties
    {
        public Rarity rarity = Rarity.Common;

        public CompProperties_LabelColored()
        {
            compClass = typeof(CompLabelColored);
        }
    }
}
