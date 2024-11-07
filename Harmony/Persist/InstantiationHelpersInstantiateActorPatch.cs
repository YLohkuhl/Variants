using HarmonyLib;
using Variants.Persist;

namespace Variants.Harmony
{
    [HarmonyPatch(typeof(InstantiationHelpers), nameof(InstantiationHelpers.InstantiateActor))]
    internal static class InstantiationHelpersInstantiateActorPatch
    {
        public static void Postfix(ref GameObject __result)
        {
            var id = __result?.GetComponent<Identifiable>()?.identType;
            if (!id)
                return;

            if (VacuumItemPatch.HasVariant.IsNotNull())
            {
                var v = VariantPersistence.Instance.Plugin.Variant;

                int count = VacuumItemPatch.HasVariant.Value;

                ///

                var applicator = __result.GetComponent<VariantAppearanceApplicator>();
                applicator.Appearance = VariantAppearances.Find(x => x.PersistentId == v.PlayerAmmo[id.ReferenceId][count]);

                if (applicator.Appearance.IsNull())
                    VariantLogger.Warning($"[{__result.name}] Failed to find variant appearance. ({v.PlayerAmmo[id.ReferenceId][count]})");

                ///

                if (v.PlayerAmmo[id.ReferenceId].Count > 1)
                    v.PlayerAmmo[id.ReferenceId].Remove(count);
                else
                    v.PlayerAmmo.Remove(id.ReferenceId);

                ///

                VacuumItemPatch.HasVariant = null;
            }

            if (SiloCatcherPatch.HasVariant.IsNotNull())
            {
                var v = VariantPersistence.Instance.Plugin.Variant;

                int count = SiloCatcherPatch.HasVariant.Item2;

                string plotId = SiloCatcherPatch.HasVariant.Item1;

                ///

                var applicator = __result.GetComponent<VariantAppearanceApplicator>();
                applicator.Appearance = VariantAppearances.Find(x => x.PersistentId == v.SiloAmmos[plotId][id.ReferenceId][count]);

                if (applicator.Appearance.IsNull())
                    VariantLogger.Warning($"[{__result.name}] Failed to find variant appearance. ({v.SiloAmmos[plotId][id.ReferenceId][count]})");

                ///

                if (v.SiloAmmos[plotId][id.ReferenceId].Count > 1)
                    v.SiloAmmos[plotId][id.ReferenceId].Remove(count);

                else if (v.SiloAmmos[plotId].Count > 1)
                    v.SiloAmmos[plotId].Remove(id.ReferenceId);

                else
                    v.SiloAmmos.Remove(plotId);

                ///

                SiloCatcherPatch.HasVariant = null;
            }
        }
    }
}