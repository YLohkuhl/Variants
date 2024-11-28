using HarmonyLib;
using Il2CppMonomiPark.SlimeRancher.Pedia;
using Il2CppMonomiPark.SlimeRancher.UI.Pedia;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Variants.Harmony
{
    [HarmonyPatch(typeof(PediaRoot), nameof(PediaRoot.ShowEntry))]
    internal static class PediaRootShowEntryPatch
    {
        public static PediaCategory Category => LookupDirectorAwakePatch.VariantsCategory;
        public static PediaRuntimeCategory RuntimeCategory => LookupDirectorAwakePatch.VariantsRuntimeCategory;

        public static PediaEntry Locked => LookupDirectorAwakePatch.LockedEntry;
        public static PediaEntry LockedSlime => LookupDirectorAwakePatch.LockedSlimeEntry;

        public static bool Prefix(PediaRoot __instance, PediaEntry entry)
        {
            var dict = VariantPedias.FirstOrDefault(x => x.Value.FirstOrDefault(x => x.Equals(entry)));

            if (dict.IsNotNull())
            {
                var pediaScreen = __instance.ScreenForCategory(Get<PediaCategory>("Resources").GetRuntimeCategory())?.TryCast<PediaSlimeAndResourceScreen>();
                var identifiable = GetReferencedType(dict.Key);

                if (!pediaScreen || !identifiable)
                    return false;

                ///

                if (identifiable.TryCast<SlimeDefinition>())
                    Category._lockedEntry = LockedSlime;
                else
                    Category._lockedEntry = Locked;

                Category._items = VariantPedias[identifiable.ReferenceId];
                RuntimeCategory._items = new();

                foreach (var x in VariantPedias[identifiable.ReferenceId])
                    RuntimeCategory.AddDynamicItem(x);

                ///

                pediaScreen.SetCategory(RuntimeCategory);
                __instance._nextScreen = pediaScreen;
                return false;
            }

            return true;
        }
    }
}
