using HarmonyLib;
using Il2CppMonomiPark.SlimeRancher.UI;

namespace Variants.Harmony
{
    [HarmonyPatch(typeof(TargetingUI), nameof(TargetingUI.GetIdentifiableTarget))]
    internal static class TargetingUIGetIdentifiableTargetPatch
    {
        public static bool Prefix(TargetingUI __instance, GameObject gameObject, ref bool __result)
        {
            var applicator = gameObject?.GetComponent<VariantAppearanceApplicator>();
            if (!applicator)
                return true;

            if (applicator.IsApplied && applicator.Appearance.LocalizedName.IsNotNull())
            {
                __result = true;
                __instance._nameLocString = applicator.Appearance.LocalizedName;
                __instance._infoLocString = __instance.GetIdentifiableInfoText(applicator.IdentifiableActor.identType);
                return false;
            }

            return true;
        }
    }
}
