using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Variants.Persist;

namespace Variants.Harmony.Persist
{
    [HarmonyPatch(typeof(LandPlotLocation), nameof(LandPlotLocation.Replace))]
    internal static class LandPlotLocationReplacePatch
    {
        public static void Prefix(LandPlotLocation __instance)
        {
            var v = VariantPersistence.Instance.Plugin.Variant;
            if (v.SiloAmmos.ContainsKey(__instance._id))
                v.SiloAmmos.Remove(__instance._id);
        }
    }
}
