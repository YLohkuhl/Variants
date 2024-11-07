using HarmonyLib;
using Il2CppMonomiPark.SlimeRancher.Pedia;
using Il2CppMonomiPark.SlimeRancher.UI.Framework.CommonControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Variants.Harmony
{
    [HarmonyPatch(typeof(PediaDirector), nameof(PediaDirector.Awake))]
    internal static class PediaDirectorAwakePatch
    {
        public static void Prefix(PediaDirector __instance)
        {
            InputLegendConfiguration inputConfig = Get<InputLegendConfiguration>("BlueprintItemsInputLegend");

            inputConfig._hints = inputConfig._hints.ToArray().AddToArray(new()
            {
                Label = LocalizationDirectorLoadTablesPatch.AddTranslation("UI", "b.view_variants", "View variants"),
                InputEvent = InputDirectorAwakePatch.InputEvent
            });
        }
    }
}
