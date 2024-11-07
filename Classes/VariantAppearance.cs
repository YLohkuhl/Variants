using Il2CppMonomiPark.SlimeRancher.Pedia;
using MelonLoader;
using UnityEngine.Localization;
using HarmonyLib;

namespace Variants
{
    public class VariantAppearance
    {
        /// <summary>
        /// Properly initializes the <see cref="VariantAppearance"/>.
        /// </summary>
        public void Init()
        {
            if (!PersistentId.Equals("VariantAppearance."))
            {
                if (VariantAppearances.Find(x => x.PersistentId.Equals(PersistentId)).IsNull())
                {
                    VariantAppearances.Add(this);

                    if (PediaEntry)
                    {
                        if (IdentifiableRef.IsNotNull() && !IdentifiableRef.Equals(string.Empty))
                        {
                            if (VariantPedias.ContainsKey(IdentifiableRef))
                                VariantPedias[IdentifiableRef] = VariantPedias[IdentifiableRef].AddToArray(PediaEntry);
                            else
                                VariantPedias.TryAdd(IdentifiableRef, [PediaEntry]);
                        }
                        else
                            VariantLogger.Warning($"[{PersistentId}] Variant Appearances require a valid `IdentifiableRef` (IdentifiableType.ReferenceId) to be set for viewing variant pedia entries. Ensure the identifiable already has it's own entry as well.");
                    }
                }
                else
                    VariantLogger.Error($"[{PersistentId}] Variant Appearances cannot have duplicate Persistent IDs. Rename the appearance to avoid any future conflicts.");
                return;
            }
            VariantLogger.Error($"[{PersistentId}] Variant Appearances require a unique name to be set for persistent identification.");
        }

        ///

        public string PersistentId => "VariantAppearance." + Name.Trim().Replace(" ", "");

        ///

        public LocalizedString LocalizedName { get; set; }

        public FixedPediaEntry PediaEntry { get; set; }

        public string IdentifiableRef { get; set; }

        public string Name { get; set; }

        public Color Color { get; set; }

        ///

        public SlimeAppearance.Palette Palette { get; set; }

        ///

        public VariantDesign Design { get; set; }

        ///

        public class VariantDesign
        {
            internal Dictionary<string, Material> RuntimeMaterials = [];

            internal Dictionary<SlimeFace.SlimeExpression, Material[]> RuntimeExpressions = [];

            ///

            public VariantMesh[] Meshes { get; set; }

            public VariantMaterial[] Materials { get; set; }

            public VariantExpression[] Expressions { get; set; }
        }
    }
}
