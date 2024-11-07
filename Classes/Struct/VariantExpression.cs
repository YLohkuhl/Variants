using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Variants
{
    public struct VariantExpression
    {
        public VariantMaterial Eyes { get; set; }

        public VariantMaterial Mouth { get; set; }

        public SlimeFace.SlimeExpression Expression { get; set; }
    }
}
