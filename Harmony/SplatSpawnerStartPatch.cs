using HarmonyLib;
using Il2CppMonomiPark.SlimeRancher;

namespace Variants.Harmony
{
    [HarmonyPatch(typeof(SplatSpawner), nameof(SplatSpawner.Start))]
    internal static class SplatSpawnerStartPatch
    {
        public static void Postfix(SplatSpawner __instance)
        {
            VariantAppearanceApplicator applicator = __instance.GetComponentInParent<VariantAppearanceApplicator>();
            if (applicator && applicator.IsApplied)
            {
                __instance.colors = applicator.Appearance.Palette;
                __instance.splatColor = applicator.Appearance.Color;
            }
        }
    }
}
