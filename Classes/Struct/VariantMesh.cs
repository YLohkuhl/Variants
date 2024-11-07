using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Variants
{
    public struct VariantMesh
    {
        /// <summary>
        /// Keep in mind when setting the <see cref="UnityEngine.Transform"/> name, it will check for anything that contains this name. It is best not to be too specific as to let it check for LODs.
        /// </summary>
        public string Transform { get; set; }

        public Mesh Mesh { get; set; }
    }
}
