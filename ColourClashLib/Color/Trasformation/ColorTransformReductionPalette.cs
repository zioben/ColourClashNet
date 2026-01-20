using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Transformation
{
    public class ColorTransformReductionPalette : ColorTransformBase
    {
        //-------------------------------------------------------------
        // Generic Fixed Palette Management 
        //-------------------------------------------------------------
        public ColorTransformReductionPalette()
        {
            Type = ColorTransformType.ColorReductionGenericPalette;
            Description = "Color palette trasformation";
        }

        protected override ColorTransformResults CreateTransformationMap(CancellationToken oToken = default)
        {
            TransformationMap.Reset();
            var rgbList = SourceData.ColorPalette.ToList();
            Parallel.ForEach(rgbList, rgb =>
            {
                TransformationMap.Add(rgb, ColorIntExt.GetNearestColor(rgb, FixedPalette, this.ColorDistanceEvaluationMode));
            });
            //Ferificed OK
            //var t = TransformationMap.rgbTransformationMap.Values.Distinct().ToList();
            return ColorTransformResults.CreateValidResult();
        }
    }
}

