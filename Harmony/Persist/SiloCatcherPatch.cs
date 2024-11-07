using HarmonyLib;
using MelonLoader;
using Variants.Persist;

namespace Variants.Harmony
{
    [HarmonyPatch(typeof(SiloCatcher))]
    internal static class SiloCatcherPatch
    {
        public static Tuple<string, int> HasVariant;

        ///

        [HarmonyPostfix]
        [HarmonyPatch(nameof(SiloCatcher.OnTriggerEnter))]
        public static void OnTriggerEnter(SiloCatcher __instance, Collider collider)
        {
            var applicator = collider?.gameObject?.GetComponent<VariantAppearanceApplicator>();
            if (!applicator)
                return;

            var ammo = __instance._storageSilo.GetRelevantAmmo();
            var id = ammo?.GetSelectedId();

            if (!id)
                return;

            var location = __instance.GetComponentInParent<LandPlotLocation>();
            if (!location)
                return;

            ///

            if (applicator.IsApplied)
            {
                int count = ammo.GetSlotCount(ammo.SelectedAmmoIndex);
                if (count < 1)
                    return;

                ///

                var v = VariantPersistence.Instance.Plugin.Variant;

                if (!v.SiloAmmos.ContainsKey(location._id))
                    v.SiloAmmos.TryAdd(location._id, new() { { id.ReferenceId, new() { { count, applicator.Appearance.PersistentId } } } });

                else if (!v.SiloAmmos[location._id].ContainsKey(id.ReferenceId))
                    v.SiloAmmos[location._id].TryAdd(id.ReferenceId, new() { { count, applicator.Appearance.PersistentId } });

                else
                    v.SiloAmmos[location._id][id.ReferenceId].TryAdd(count, applicator.Appearance.PersistentId);

                ///

                long actorId = applicator.IdentifiableActor.GetActorId().Value;
                VariantPersistence.Instance.Plugin.Variant.Actors.Remove(actorId);
            }
        }

        ///

        [HarmonyPrefix]
        [HarmonyPatch(nameof(SiloCatcher.OnTriggerStay))]
        public static void OnTriggerStay(SiloCatcher __instance, Collider collider)
        {
            if (!__instance.Type.Equals(SiloCatcherType.SILO_DEFAULT))
                return;

            SiloActivator componentInParent = collider.gameObject.GetComponentInParent<SiloActivator>();
            if (!componentInParent || !componentInParent.enabled)
                return;

            var ammo = __instance._storageSilo.GetRelevantAmmo();
            var id = ammo?.GetSelectedId();

            if (!id)
                return;

            var location = __instance.GetComponentInParent<LandPlotLocation>();
            if (!location)
                return;

            ///

            if (HasVariant.IsNull())
            {
                var v = VariantPersistence.Instance.Plugin.Variant;

                if (v.SiloAmmos.ContainsKey(location._id))
                {
                    if (v.SiloAmmos[location._id].ContainsKey(id.ReferenceId))
                    {
                        int count = ammo.GetSlotCount(ammo.SelectedAmmoIndex);

                        if (v.SiloAmmos[location._id][id.ReferenceId].ContainsKey(count))
                            HasVariant = new(location._id, count);
                    }
                }
            }
        }

        ///

        [HarmonyFinalizer]
        [HarmonyPatch(nameof(SiloCatcher.OnTriggerStay))]
        public static void OnTriggerStayFinalizer() => HasVariant = null;
    }
}
