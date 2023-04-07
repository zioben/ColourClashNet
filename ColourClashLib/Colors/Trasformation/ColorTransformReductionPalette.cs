﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColourClashLib.Colors.Tile;
using ColourClashNet.Colors;

namespace ColourClashNet.Colors.Transformation
{
    public class ColorTransformReductionPalette : ColorTransformBase
    {

        //-------------------------------------------------------------
        // Generic FIxed Palette injecct 
        //-------------------------------------------------------------
        public ColorTransformReductionPalette()
        {
            type = ColorTransform.ColorReductionGenericPalette;
            description = "Color palette trasformation";
        }

        protected override void CreateTrasformationMap()
        {
        }

        protected override int[,]? ExecuteTransform(int[,]? oDataSource)
        {
            if (colorPalette == null ) 
            {
                return null;    
            }
            if (oDataSource == null)
            {
                return null;
            }
            var oHashSet = new HashSet<int>();
            foreach (var rgb in oDataSource)
            {
                oHashSet.Add(rgb);
            }
            oHashSet.RemoveWhere(X => X < 0);
            foreach (var rgb in oHashSet)
            {
                colorTransformationMap.Add(rgb, rgb);
            }
            Parallel.ForEach(oHashSet, rgb =>
            {
                colorTransformationMap.rgbTransformationMap[rgb] = ColorIntExt.GetNearestColor(rgb, colorPalette, ColorDistanceEvaluationMode);
            });

            return base.ExecuteTransform(oDataSource);
        }

    }
}