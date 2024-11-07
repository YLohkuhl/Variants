using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Variants
{
    public struct VariantMaterial
    {
        /// <summary>
        /// Keep in mind when setting the <see cref="UnityEngine.Transform"/> name, it will check for anything that contains this name. It is best not to be too specific as to let it check for LODs.
        /// </summary>
        public string Transform { get; set; }

        public Shader Shader { get; set; }

        /// <summary>
        /// A premade <see cref="Material"/> to be set instead of constructing one with given properties.
        /// </summary>
        public Material Premade { get; set; }

        public string[] DisabledKeywords { get; set; }

        public string[] EnabledKeywords { get; set; }

        public Dictionary<string, object> Properties { get; set; }
    }
}
