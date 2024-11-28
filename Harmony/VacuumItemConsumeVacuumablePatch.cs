using HarmonyLib;
using Il2CppMonomiPark.SlimeRancher;
using Il2CppMonomiPark.SlimeRancher.Player;
using Il2CppMonomiPark.SlimeRancher.Player.PlayerItems;
using Il2CppMonomiPark.SlimeRancher.Slime.Tangle;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Variants.Harmony
{
    [HarmonyPatch(typeof(VacuumItem), nameof(VacuumItem.ConsumeVacuumable))]
    internal static class VacuumItemConsumeVacuumablePatch
    {
        public static void Postfix(VacuumItem __instance, GameObject gameObj)
        {
            var vacuumable = gameObj.GetComponent<Vacuumable>();
            if (!vacuumable || !vacuumable.enabled || !vacuumable.identifiable)
                return;

            Vector3 direction = gameObj.transform.position - __instance.VacOrigin.transform.position;
            Ray ray = new(__instance.VacOrigin.transform.position, direction);

            if (!Physics.Raycast(ray, __instance.MaxVacDist, Layers.Actor))
                return;

            if (vacuumable.isCaptive() && Vector3.Distance(gameObj.transform.position, ray.origin) < __instance.CaptureDist)
            {
                var applicator = gameObj.GetComponent<VariantAppearanceApplicator>();
                if (!applicator)
                    return;

                if (applicator.IsApplied && applicator.Appearance.UnlockPediaOnVac)
                {
                    var dict = VariantPedias.FirstOrDefault(x => x.Key.Equals(applicator.Appearance.IdentifiableRef));
                    if (dict.IsNull() || dict.Value.IsNull())
                        return;

                    if (dict.Value.Contains(applicator.Appearance.PediaEntry))
                        SceneContext.Instance.PediaDirector.Unlock(applicator.Appearance.PediaEntry);
                }
            }
        }
    }
}
