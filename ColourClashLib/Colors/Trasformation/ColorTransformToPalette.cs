﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColourClashNet.Colors;

namespace ColourClashNet.Colors.Transformation
{
    public abstract class ColorTransformToPalette : ColorTransformBase
    {

        public ColorTransformToPalette()
        {
            Description = "Color palette trasformation";
        }

        protected override int[,]? ExecuteTransform(int[,]? oDataSource)
        {
            if (oDataSource == null)
                return null;

            var oHashSet = new HashSet<int>();
            foreach (var rgb in oDataSource)
            {
                oHashSet.Add(rgb);
            }
            oHashSet.RemoveWhere(X => X < 0);
            foreach (var rgb in oHashSet)
            {
                TransformationColorMap.Add(rgb, rgb);
            }
            Parallel.ForEach(oHashSet, rgb =>
            {
                TransformationColorMap[rgb] = ColorIntExt.GetNearestColor(rgb, SourceColorPalette, ColorDistanceEvaluationMode);
            });

            return base.ExecuteTransform(oDataSource);
        }

    }
}
