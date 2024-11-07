
namespace Variants.Persist
{
    public class VariantV01 : VariantPersistentData
    {
        public override uint Version => 1;

        public VariantV01()
        {
            Actors = [];
            PlayerAmmo = [];
            SiloAmmos = [];
        }

        ///

        public Dictionary<long, string> Actors;

        public Dictionary<string, Dictionary<int, string>> PlayerAmmo;

        public Dictionary<string, Dictionary<string, Dictionary<int, string>>> SiloAmmos;

        ///

        public override void Load() { }

        public void Cleanup()
        {
            AutoSaveDirector autoSaveDirector = GameContext.Instance.AutoSaveDirector;

            var actors = autoSaveDirector.SavedGame.GameState.Actors;
            var referenceLookup = autoSaveDirector.SavedGame.persistenceIdToIdentifiableType._referenceIdProviderLookup;
            var plots = autoSaveDirector.SavedGame.GameState.Ranch.Plots;

            foreach (var key in Actors.Keys)
            {
                var actor = actors.ToArray().FirstOrDefault(x => x.ActorId == key);
                if (actor.IsNull())
                {
                    Actors.Remove(key);
                    VariantLogger.Warning($"[{key}] Could not find this Actor; cleaning up!");
                }
            }

            foreach (var key in PlayerAmmo.Keys)
            {
                var contains = referenceLookup.ContainsKey(key);
                if (!contains)
                {
                    PlayerAmmo.Remove(key);
                    VariantLogger.Warning($"[Player] [{key}] Could not find this Identifiable; cleaning up!");
                }
            }

            foreach (var pair in SiloAmmos)
            {
                var plot = plots.ToArray().FirstOrDefault(x => x.ID == pair.Key);
                if (plot.IsNull())
                {
                    SiloAmmos.Remove(pair.Key);
                    VariantLogger.Warning($"[{pair.Key}] Could not find this Plot; cleaning up!");
                }
                else
                {
                    foreach (var value in pair.Value)
                    {
                        var contains = referenceLookup.ContainsKey(value.Key);
                        if (!contains)
                        {
                            SiloAmmos[pair.Key].Remove(value.Key);
                            VariantLogger.Warning($"[Silo] [{value.Key}] Could not find this Identifiable; cleaning up!");
                        }
                    }
                }
            }

            ///

            VariantLogger.Msg("Requested data cleanup; cleaned up.");
        }
    }
}
