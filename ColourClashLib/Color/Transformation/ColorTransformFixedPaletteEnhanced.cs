using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Transformation
{
    /// <summary>
    /// Fixed palette mapping extended with the pairwise mean of every two palette colors.
    /// <para/>
    /// The mean colors represent the visual result of a 50% checkerboard dither between the two colors.
    /// </summary>
    public class ColorTransformFixedPaletteEnhanced : ColorTransformBase
    {
        /// <summary>
        /// Creates the enhanced palette transformation.
        /// </summary>
        public ColorTransformFixedPaletteEnhanced()
        {
            Type = ColorTransformType.ColorReductionEnhancedPalette;
            Description = "Enhanced color palette transformation";
        }


        /// <summary>
        /// Builds the enhanced palette and maps each source color to its nearest entry.
        /// </summary>
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
            //Parallel.ForEach(rgbList, rgb =>
            foreach (var rgb in rgbList)    
            {
                TransformationMap.Add(rgb, ColorIntExt.GetNearestColor(rgb, paletteEnhance, this.ColorDistanceEvaluationMode));
            }//);
            return ColorTransformResult.CreateValidResult();
        }
    }
}

