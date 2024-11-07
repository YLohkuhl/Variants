using HarmonyLib;
using Il2CppMonomiPark.SlimeRancher.Player.PlayerItems;
using Variants.Persist;

namespace Variants.Harmony
{
    [HarmonyPatch(typeof(VacuumItem))]
    internal static class VacuumItemPatch
    {
        public static int? HasVariant;

        ///

        [HarmonyPrefix]
        [HarmonyPatch(nameof(VacuumItem.Expel), [typeof(GameObject), typeof(bool), typeof(float), typeof(SlimeAppearance.AppearanceSaveSet)])]
        public static void Expel(VacuumItem __instance, GameObject toExpel)
        {
            var id = toExpel?.GetComponent<IdentifiableActor>()?.identType;
            if (!id)
                return;

            ///

            if (HasVariant.IsNull())
            {
                var v = VariantPersistence.Instance.Plugin.Variant;

                if (v.PlayerAmmo.ContainsKey(id.ReferenceId))
                {
                    int count = __instance._player.Ammo.Slots.FirstOrDefault(x => x.Id.Equals(id)).Count;

                    if (v.PlayerAmmo[id.ReferenceId].ContainsKey(count))
                        HasVariant = count;
                }
            }
        }

        ///

        [HarmonyFinalizer]
        [HarmonyPatch(nameof(VacuumItem.Expel), [typeof(GameObject), typeof(bool), typeof(float), typeof(SlimeAppearance.AppearanceSaveSet)])]
        public static void ExpelFinalizer() => HasVariant = null;
    }
}
