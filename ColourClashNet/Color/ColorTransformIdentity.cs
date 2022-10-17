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
            oColorTransformation.Clear();
            foreach (var kvp in oColorHistogram)
            {
                oColorTransformation.Add(kvp.Key, kvp.Key);
            }
            ColorsUsed = oColorHistogram.Count();
        }
    }
}
