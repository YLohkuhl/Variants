using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Variants.Harmony
{
    [HarmonyPatch(typeof(SlimeAppearanceApplicator), nameof(SlimeAppearanceApplicator.SetExpression))]
    internal static class SlimeAppearanceApplicatorSetExpressionPatch
    {
        public static bool Prefix(SlimeAppearanceApplicator __instance, ref SlimeFace.SlimeExpression slimeExpression)
        {
            var vApplicator = __instance.gameObject?.GetComponent<VariantAppearanceApplicator>();
            if (!vApplicator)
                return true;

            var runtime = vApplicator.Appearance?.Design?.RuntimeExpressions;
            if (runtime.IsNull())
                return true;

            ///

            if (vApplicator.IsApplied && runtime.ContainsKey(slimeExpression))
            {
                bool result = false;

                int i = 0;
                foreach (var renderer in __instance._faceRenderers)
                {
                    Material eyes = runtime[slimeExpression][0];
                    Material mouth = runtime[slimeExpression][1];

                    int index = renderer.Renderer.sharedMaterials.Length - 2;

                    if (renderer.ShowEyes && eyes)
                    {
                        renderer.Renderer.sharedMaterials[index] = eyes;
                        result = true;
                    }

                    if (renderer.ShowMouth && mouth)
                    {
                        index++;
                        if (!renderer.ShowEyes)
                            index--;
                        renderer.Renderer.sharedMaterials[index] = mouth;
                        result = true;
                    }

                    i++;
                    VariantLogger.Msg($"Logs{i}");
                }

                if (result)
                    return false;

                return true;
            }

            return true;
        }
    }
}
