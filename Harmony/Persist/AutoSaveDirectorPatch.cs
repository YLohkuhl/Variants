using HarmonyLib;
using Il2CppSystem.Linq;
using MelonLoader;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using Variants.Persist;

namespace Variants.Harmony
{
    [HarmonyPatch(typeof(AutoSaveDirector))]
    internal static class AutoSaveDirectorPatch
    {
        public static string GameSavePath;

        public static VariantPersistence VariantPersistence;

        ///

        [HarmonyPrefix]
        [HarmonyPatch(nameof(AutoSaveDirector.DeleteGame))]
        public static void DeleteGame() 
        {
            bool exists = File.Exists(VariantPersistence.PersistenceDataPath);
            if (exists)
            {
                File.Delete(VariantPersistence.PersistenceDataPath);
                VariantLogger.Msg("Requested data deletion; deleted.");
            }
        }

        ///

        [HarmonyPostfix]
        [HarmonyPatch(nameof(AutoSaveDirector.SaveGame))]
        public static void SaveGame()
        {
            VariantPersistence.Instance.Plugin.Write();
            VariantLogger.Msg("Requested data save; saved.");
        }

        ///

        [HarmonyPostfix]
        [HarmonyPatch(nameof(AutoSaveDirector.BeginLoad))]
        public static void BeginLoad(AutoSaveDirector __instance, string gameName)
        {
            if (GameSavePath.IsNull() || GameSavePath.Equals(string.Empty))
                GameSavePath = __instance.StorageProvider?.TryCast<FileStorageProvider>().savePath;

            VariantPersistence.PersistenceDataPath = Path.Combine(GameSavePath, $"{gameName}.variant");
            VariantPersistence = new VariantPersistence();

            VariantPersistence.Instance.Plugin.Variant.Cleanup();
        }
    }
}
