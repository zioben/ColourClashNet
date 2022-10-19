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
            foreach (var kvp in oColorHistogram)
            {
                oColorsPalette.Add(kvp.Key);
                oColorTransformation.Add(kvp.Key, kvp.Key);
            }
        }

        public override int[,] Transform(int[,] oDataSource)
        {
            return base.TransformBase(oDataSource); 
        }

    }
}
