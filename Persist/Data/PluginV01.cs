using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Variants.Persist
{
    public class PluginV01 : VariantPersistentData
    {
        public override uint Version => 1;

        public PluginV01() => Variant = new();

        ///

        public override void Load()
        {
            PluginV01 plugin = base.Load<PluginV01>();

            Variant.Actors = plugin.Variant.Actors;
            Variant.PlayerAmmo = plugin.Variant.PlayerAmmo;
            Variant.SiloAmmos = plugin.Variant.SiloAmmos;

            ///

            VariantLogger.Msg("Requested data load...");
        }

        public override void Cleanup() { }
        
        ///

        public VariantV01 Variant { get; private set; }
    }
}
