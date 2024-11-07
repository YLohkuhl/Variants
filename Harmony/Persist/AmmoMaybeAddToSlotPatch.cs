using HarmonyLib;
using Variants.Persist;

namespace Variants.Harmony
{
    [HarmonyPatch(nameof(Ammo), nameof(Ammo.MaybeAddToSlot))]
    internal static class AmmoMaybeAddToSlotPatch
    {
        public static void Postfix(IdentifiableType id, Identifiable identifiable, ref bool __result)
        {
            if (!__result)
                return;

            var applicator = identifiable?.GetComponent<VariantAppearanceApplicator>();
            if (!applicator)
                return;

            ///

            if (applicator.IsApplied)
            {
                Ammo.Slot slot = SceneContext.Instance.PlayerState.Ammo.Slots?.FirstOrDefault(x => x.Id == id);
                if (slot.IsNull())
                    return;

                ///

                var v = VariantPersistence.Instance.Plugin.Variant;

                if (!v.PlayerAmmo.ContainsKey(id.ReferenceId))
                    v.PlayerAmmo.TryAdd(id.ReferenceId, new() { { slot.Count, applicator.Appearance.PersistentId } });
                else
                    v.PlayerAmmo[id.ReferenceId].TryAdd(slot.Count, applicator.Appearance.PersistentId);

                ///

                long actorId = applicator.IdentifiableActor.GetActorId().Value;
                VariantPersistence.Instance.Plugin.Variant.Actors.Remove(actorId);
            }
        }
    }
}
