using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColourClashLib;
using ColourClashNet.Colors;

namespace ColourClashNet.Colors.Transformation
{
    public class ColorTransformIdentity : ColorTransformBase
    {
        static string sClass = nameof(ColorTransformIdentity);
        public ColorTransformIdentity()
        {
            type = ColorTransform.ColorIdentity;
            description = "1:1 Color transformation";
        }

        protected override void CreateTrasformationMap()
        {
            string sMethod = nameof(CreateTrasformationMap);
            if (ColorDefaults.Trace)
                Trace.TraceInformation($"{sClass}.{sMethod} ({type}) : Creating trasformation map");

            foreach (var kvp in colorHistogram.rgbHistogram)
            {
                colorTransformationMap.Add(kvp.Key, kvp.Key);
            }
        }
    }
}
