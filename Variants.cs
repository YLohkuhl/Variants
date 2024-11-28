global using Il2Cpp;
global using UnityEngine;

global using static _;
global using static Variants.Variants;
using Il2CppMonomiPark.SlimeRancher.Pedia;
using MelonLoader;

[assembly: HarmonyDontPatchAll]

[assembly: MelonInfo(typeof(Variants.Variants), "Variants", "1.0.0", "YLohkuhl", "https://www.nexusmods.com/slimerancher2/mods/87")]
[assembly: MelonGame("MonomiPark", "SlimeRancher2")]
[assembly: MelonColor(0, 131, 105, 228)]

namespace Variants
{
    public class Variants : MelonPlugin
    {
        public static MelonLogger.Instance VariantLogger { get; private set; }

        public static List<VariantAppearance> VariantAppearances { get; private set; }

        public static Dictionary<string, PediaEntry[]> VariantPedias { get; private set; }

        public override void OnInitializeMelon()
        {
            VariantLogger = new("Variants", System.Drawing.Color.FromArgb(0, 131, 105, 228));

            VariantAppearances = [];
            VariantPedias = [];
            
            HarmonyInstance.PatchAll();
        }
    }
}
