using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Transformation
{
    public class ColorTransformFixedPaletteEnhanced : ColorTransformBase
    {
        //-------------------------------------------------------------
        // Generic Fixed Palette Management 
        //-------------------------------------------------------------
        public ColorTransformFixedPaletteEnhanced()
        {
            Type = ColorTransformType.ColorReductionEnhancedPalette;
            Description = "Color palette trasformation";
        }


        protected override ColorTransformResult CreateTransformationMap(CancellationToken token = default)
        {
            TransformationMap.Reset();

            var rgbList = ImageSource.ColorPalette.ToList();
            Palette paletteEnhance = new ();
            paletteEnhance.Create(ReferencePalette);
            // Enhanced colors are the pairwise means of the reference palette colors
            var refList = ReferencePalette.ToList();
            for (int i = 0; i < refList.Count; i++)
            {
                for (int j = i + 1; j < refList.Count; j++)
                {
                    paletteEnhance.Add(ColorIntExt.GetColorMean(refList[i], refList[j]));
                }
            }
            //Parallel.ForEach(rgbListEnhance, rgb =>
            foreach (var rgb in rgbList)    
            {
                TransformationMap.Add(rgb, ColorIntExt.GetNearestColor(rgb, paletteEnhance, this.ColorDistanceEvaluationMode));
            }//);
            //Ferificed OK
            //var t = TransformationMap.RgbTransformationMap.Values.Distinct().ToList();
            return ColorTransformResult.CreateValidResult();
        }
    }
}

