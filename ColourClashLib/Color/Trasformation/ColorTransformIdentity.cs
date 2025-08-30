using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColourClashNet.Color;
using ColourClashNet.Log;

namespace ColourClashNet.Color.Transformation
{
    public class ColorTransformIdentity : ColorTransformBase
    {
        static string sClass = nameof(ColorTransformIdentity);
        public ColorTransformIdentity()
        {
            Type = ColorTransformType.ColorIdentity;
            Description = "1:1 Color transformation";
        }

        protected override void CreateTrasformationMap()
        {
            string sMethod = nameof(CreateTrasformationMap);
            LogMan.Trace(sClass, sMethod, $"{Type} : Clearing trasformation map");

            foreach (var kvp in OutputHistogram.rgbHistogram)
            {
                ColorTransformationMapper.Add(kvp.Key, kvp.Key);
            }
        }
    }
}
