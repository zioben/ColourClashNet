﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Colors
{
    public class ColorTransformToPalette : ColorTransformBase
    {

        public ColorTransformToPalette()
        {
            Name = "Identity";
            Description = "1:1 Color transformation";
        }

        public ColorDistanceEvaluationMode ColorDistanceMode { get; set; }

        protected override void CreateTrasformationMap()
        {
            foreach (var kvp in oColorHistogram)
            {
                hashColorsPalette.Add(kvp.Key);
            }
        }

        public override int[,]? Transform(int[,]? oDataSource)
        {
            if (oDataSource == null)
                return null;

            var oHasSet = new  HashSet<int>();

            foreach (var rgb in oDataSource)
            {
                oHasSet.Add(rgb);
            }
            oHasSet.Remove(-1);
            foreach (var rgb in oHasSet)
            {
                oColorTransformationMap.Add(rgb, rgb);
            }
            var oColorList = oColorTransformationPalette;
            Parallel.ForEach( oHasSet, rgb =>
            {
                oColorTransformationMap[rgb] = ColorTransformBase.GetNearestColor(rgb, oColorTransformationPalette, ColorDistanceMode);
            });

            return ApplyTransform(oDataSource); 
        }

    }
}
