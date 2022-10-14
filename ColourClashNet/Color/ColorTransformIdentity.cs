using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Colors
{
    public class ColorTransformIdentity : ColorTransformBase
    {
        protected override void BuildTrasformation()
        {
            DictColorTransformation.Clear();
            foreach (var kvp in DictColorHistogram)
            {
                DictColorTransformation.Add(kvp.Key, kvp.Key);
            }
            ColorsUsed = DictColorHistogram.Count();
        }
    }
}
