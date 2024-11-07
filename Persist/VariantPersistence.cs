using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Variants.Persist
{
    public class VariantPersistence
    {
        public static VariantPersistence Instance { get; private set; }

        public static string PersistenceDataPath { get; set; }

        ///

        public VariantPersistence()
        {
            if (Instance.IsNotNull() && Instance != this)
            {
                VariantLogger.Warning("Persistence instance already exists. Retreating..");
                return;
            }
            Instance = this;

            ///

            Instance.Plugin = new();
            Instance.Plugin.Load();
        }

        ///

        public static bool Exists() => File.Exists(PersistenceDataPath);

        ///

        public PluginV01 Plugin { get; private set; }
    }
}
