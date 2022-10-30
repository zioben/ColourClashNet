using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Colors
{
    public class ColorTransformIdentity : ColorTransformBase
    {

        public ColorTransformIdentity()
        {
            Name = "Identity";
            Description = "1:1 Color transformation";
        }

        protected override void CreateTrasformationMap()
        {
            foreach (var kvp in oColorHistogram)
            {
                oColorTransformationMap.Add(kvp.Key, kvp.Key);
                hashColorsPalette.Add(kvp.Key);
            }
        }

        public override int[,]? Transform(int[,]? oDataSource )
        {
            return ApplyTransform(oDataSource); 
        }

    }
}
