using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color
{
    public class ColorTransformIdentity : ColorTransformBase
    {
        protected override void BuildTrasformation()
        {
            DictTransform.Clear();
            foreach (var kvp in DictHistogram)
            {
                DictTransform.Add(kvp.Key, kvp.Key);
            }
        }
    }
}
