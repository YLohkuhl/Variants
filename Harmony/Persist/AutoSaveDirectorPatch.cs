using HarmonyLib;
using Il2CppSystem.Collections;
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
        public static string GameSavePath => GameContext.Instance.AutoSaveDirector.StorageProvider.TryCast<FileStorageProvider>().savePath;

        public static VariantPersistence VariantPersistence;

        ///

        [HarmonyPostfix]
        [HarmonyPatch(nameof(AutoSaveDirector.DeleteGame))]
        public static void DeleteGame()
        {
            bool exists = File.Exists(VariantPersistence.PersistenceDataPath);
            if (exists)
            {
                File.Delete(VariantPersistence.PersistenceDataPath);
                VariantLogger.Msg("Requested data deletion...");
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(AutoSaveDirector.SaveGame))]
        public static void SaveGame()
        {
            VariantPersistence.Instance?.Plugin.Write();
            VariantLogger.Msg("Requested data save...");
        }

        ///

        [HarmonyPostfix]
        [HarmonyPatch(typeof(AutoSaveDirector._LoadSave_Coroutine_d__86), "MoveNext")]
        public static void LoadSave_Coroutine(AutoSaveDirector._LoadSave_Coroutine_d__86 __instance) => SetupAndLoadVariantPersistence(__instance.__4__this, __instance.gameName);

        [HarmonyPostfix]
        [HarmonyPatch(typeof(AutoSaveDirector._LoadNewGame_Coroutine_d__52), "MoveNext")]
        public static void LoadNewGame_Coroutine(AutoSaveDirector._LoadNewGame_Coroutine_d__52 __instance)
        {
            string gameName = __instance.__4__this.GetGameSaveFileName(__instance.__4__this.GetGameSaveDisplayName(__instance.metadata.saveSlotIndex));
            SetupAndLoadVariantPersistence(__instance.__4__this, gameName, true);
        }

        ///

        public static void SetupAndLoadVariantPersistence(AutoSaveDirector __instance, string gameName, bool newGame = false)
        {
            MelonLogger.Msg("0");
            VariantPersistence.PersistenceDataPath = Path.Combine(GameSavePath, $"{gameName}.variant");

            if (VariantPersistence.IsNull())
                VariantPersistence = new VariantPersistence();
            else
                VariantPersistence.Plugin.Load();

            if (!newGame)
                VariantPersistence.Plugin.Variant.Cleanup();
        }
    }
}
