using HarmonyLib;
using Il2CppMonomiPark.SlimeRancher.Pedia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Variants.Harmony
{
    [HarmonyPatch(typeof(LookupDirector), nameof(LookupDirector.Awake))]
    internal static class LookupDirectorAwakePatch
    {
        public static PediaCategory VariantsCategory;
        public static PediaRuntimeCategory VariantsRuntimeCategory;

        public static PediaEntry LockedEntry;
        public static PediaEntry LockedSlimeEntry;

        public static void Prefix(LookupDirector __instance)
        {
            if (VariantsCategory.IsNull())
            {
                var icon = AB.variants.LoadAsset("iconPediaVariants").TryCast<Texture2D>();
                icon.hideFlags |= HideFlags.HideAndDontSave;
                icon.mipMapBias = -1;

                VariantsCategory = ScriptableObject.CreateInstance<PediaCategory>();
                VariantsCategory.hideFlags |= HideFlags.HideAndDontSave;
                VariantsCategory.name = "Variants";

                VariantsCategory._icon = icon.ToSprite();
                VariantsCategory._title = LocalizationDirectorLoadTablesPatch.AddTranslation("UI", "b.variants", "Variants");

                VariantsRuntimeCategory = new PediaRuntimeCategory(VariantsCategory);

                ///

                LockedEntry = Get<PediaEntry>("Locked");
                LockedSlimeEntry = Get<PediaEntry>("Locked Slime");
            }
        }
    }
}
