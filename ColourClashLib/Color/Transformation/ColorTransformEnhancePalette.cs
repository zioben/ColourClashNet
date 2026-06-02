using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Transformation
{
    public class ColorTransformEnhancePalette : ColorTransformBase
    {
        //-------------------------------------------------------------
        // Generic Fixed Palette Management 
        //-------------------------------------------------------------
        public ColorTransformEnhancePalette()
        {
            Type = ColorTransformType.ColorReductionGenericPalette;
            Description = "Color palette trasformation";
        }

        protected override ColorTransformResult CreateTransformationMap(CancellationToken oToken = default)
        {
            TransformationMap.Reset();

            var rgbList = ImageSource.ColorPalette.ToList();
            ColourClashNet.Color.Palette paletteEnhance = new ();
            paletteEnhance.Create(PriorityPalette);
            for (int i = 0; i < PriorityPalette.Count; i++)
            {
                for (int j = i + 1; j < PriorityPalette.Count; j++)
                {
                    paletteEnhance.Add(ColorIntExt.GetColorMean(rgbList[i], rgbList[j]));
                }
            }
            //Parallel.ForEach(rgbListEnhance, rgb =>
            foreach (var rgb in rgbList)    
            {
                TransformationMap.Add(rgb, ColorIntExt.GetNearestColor(rgb, paletteEnhance, this.ColorDistanceEvaluationMode));
            }//);
            //Ferificed OK
            //var t = TransformationMap.rgbTransformationMap.Values.Distinct().ToList();
            return ColorTransformResult.CreateValidResult();
        }
    }
}

