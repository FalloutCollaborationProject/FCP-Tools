using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Verse;

namespace Tent
{
    [StaticConstructorOnStartup]
    public class MainHarmonyInstance : Mod
    {
        public MainHarmonyInstance(ModContentPack content) : base(content)
        {
            var harmony = new Harmony("com.otters.rimworld.mod.Tents");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            CompatibilityPatches.ExecuteCompatibilityPatches(harmony);
        }
    }
}   
