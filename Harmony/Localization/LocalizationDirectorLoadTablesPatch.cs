using Il2CppMonomiPark.SlimeRancher.Script.Util;
using Il2CppMonomiPark.SlimeRancher.UI.Localization;
using MelonLoader;
using UnityEngine.Localization.Tables;
using UnityEngine.Localization;
using HarmonyLib;
using System.Collections;

namespace Variants.Harmony
{
    [HarmonyPatch(typeof(LocalizationDirector), nameof(LocalizationDirector.LoadTables))]
    internal static class LocalizationDirectorLoadTablesPatch
    {
        public static Dictionary<string, Dictionary<string, string>> AddedTranslations => [];

        public static void Postfix(LocalizationDirector __instance) => MelonCoroutines.Start(LoadTable(__instance));

        private static IEnumerator LoadTable(LocalizationDirector director)
        {
            WaitForSecondsRealtime waitForSecondsRealtime = new(0.01f);
            yield return waitForSecondsRealtime;

            foreach (Il2CppSystem.Collections.Generic.KeyValuePair<string, StringTable> keyValuePair in director.Tables)
            {
                if (AddedTranslations.TryGetValue(keyValuePair.Key, out var dictionary))
                {
                    foreach (KeyValuePair<string, string> keyValuePair2 in dictionary)
                        keyValuePair.Value.AddEntry(keyValuePair2.Key, keyValuePair2.Value);
                }
            }

            yield break;
        }

        public static LocalizedString AddTranslation(string table, string key, string localized)
        {
            if (!AddedTranslations.TryGetValue(table, out Dictionary<string, string> dictionary))
            {
                dictionary = [];
                AddedTranslations.Add(table, dictionary);
            }
            dictionary.TryAdd(key, localized);

            StringTable table2 = LocalizationUtil.GetTable(table);
            StringTableEntry stringTableEntry = table2.AddEntry(key, localized);

            return new LocalizedString(table2.SharedData.TableCollectionName, stringTableEntry.SharedEntry.Id);
        }
    }
}
