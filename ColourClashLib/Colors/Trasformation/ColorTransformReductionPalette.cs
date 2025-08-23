using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColourClashLib.Color;
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
            Type = ColorTransformType.ColorReductionGenericPalette;
            Description = "Color palette trasformation";
        }

         

       
        protected override int[,]? ExecuteTransform(int[,]? oDataSource, CancellationToken oToken )
        {
            if (OutputPalette == null ) 
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
            oToken.ThrowIfCancellationRequested();
            oHashSet.RemoveWhere(X => X < 0);
            foreach (var rgb in oHashSet)
            {
                ColorTransformationMapper.Add(rgb, rgb);
            }
            oToken.ThrowIfCancellationRequested();
            Parallel.ForEach(oHashSet, rgb =>
            {
                ColorTransformationMapper.rgbTransformationMap[rgb] = ColorIntExt.GetNearestColor(rgb, OutputPalette, ColorDistanceEvaluationMode);
                lock (OutputPalette)
                {
                    oToken.ThrowIfCancellationRequested();  
                }
            });

            return base.ExecuteTransform(oDataSource, oToken);
        }


       
    }
}
