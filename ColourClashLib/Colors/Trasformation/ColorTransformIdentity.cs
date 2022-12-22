using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColourClashNet.Colors;

namespace ColourClashNet.Colors.Transformation
{
    public class ColorTransformIdentity : ColorTransformBase
    {
        static string sClass = nameof(ColorTransformIdentity);
        public ColorTransformIdentity()
        {
            Type = ColorTransform.ColorIdentity;
            Description = "1:1 Color transformation";
        }

        protected override void CreateTrasformationMap()
        {
            string sMethod = nameof(CreateTrasformationMap);
            Trace.TraceInformation($"{sClass}.{sMethod} ({Type}) : Creating trasformation map");

            foreach (var kvp in SourceColorHistogram.rgbHistogram)
            {
                TransformationColorMap.Add(kvp.Key, kvp.Key);
            }
        }
    }
}
