using MelonLoader;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Variants.Persist
{
    [Serializable]
    public abstract class VariantPersistentData
    {
        public virtual uint Version { get; }

        public abstract void Load();

        ///

        public virtual void Write()
        {
            try
            {
                using FileStream stream = new(VariantPersistence.PersistenceDataPath, FileMode.Create);
                using StreamWriter writer = new(stream);

                string data;
                data = JsonConvert.SerializeObject(this);

                if (data.Equals(string.Empty))
                {
                    VariantLogger.Warning($"[{GetType().Name}] Data string was empty while writing data. Retreating..");
                    return;
                }

                writer.Write(data);
            }
            catch (Exception e)
            {
                VariantLogger.Error($"[{GetType().Name}] There was a problem writing data here! Please review as soon as possible.");
                VariantLogger.Error(e);
            }
        }

        ///

        public virtual T Load<T>() where T : VariantPersistentData
        {
            T container = (T)this;

            if (File.Exists(VariantPersistence.PersistenceDataPath))
            {
                try
                {
                    using FileStream stream = new(VariantPersistence.PersistenceDataPath, FileMode.Open);
                    using StreamReader reader = new(stream);

                    string data;
                    data = reader.ReadToEnd();

                    if (data.Equals(string.Empty))
                    {
                        VariantLogger.Warning($"[{GetType().Name}] Data string was empty while loading data. Retreating..");
                        return container;
                    }

                    container = JsonConvert.DeserializeObject<T>(data);
                }
                catch (Exception e)
                {
                    VariantLogger.Error($"[{GetType().Name}] There was a problem loading data here! Please review as soon as possible.");
                    VariantLogger.Error(e);
                }
            }

            return container;
        }
    }
}
