using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Colors
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

            foreach (var kvp in ColorHistogram)
            {
                ColorTransformationMap.Add(kvp.Key, kvp.Key);
                ColorTransformationPalette.Add(kvp.Key);
            }
        }
    }
}
