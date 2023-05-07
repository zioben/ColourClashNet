﻿using System;
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
            Name = ColorTransformType.ColorIdentity;
            Description = "1:1 Color transformation";
        }

        protected override void CreateTrasformationMap()
        {
            string sMethod = nameof(CreateTrasformationMap);
            if (ColorDefaults.Trace)
                Trace.TraceInformation($"{sClass}.{sMethod} ({Name}) : Creating trasformation map");

            foreach (var kvp in Histogram.rgbHistogram)
            {
                ColorTransformationMapper.Add(kvp.Key, kvp.Key);
            }
        }
    }
}
