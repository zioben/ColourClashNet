using ColourClashNet.Imaging;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Transformation
{
    /// <summary>
    /// Hercules monochrome mode: maps every color to black or white.
    /// </summary>
    public class ColorTransformHercules : ColorTransformFixedPalette
    {



        /// <summary>
        /// Creates the Hercules transformation.
        /// </summary>
        public ColorTransformHercules()
        {
            Type = ColorTransformType.ColorReductionHercules;
            Description = "Reduce color to Hercules palette";
        }

        Palette palette = new();

        /// <summary>
        /// Loads the black and white palette.
        /// </summary>
        protected override void RebuildReferencePalette()
        {
            ReferencePalette.Reset();
            ReferencePalette.Create();
            ReferencePalette.Add(ColorIntExt.FromRGB(0, 0, 0));
            ReferencePalette.Add(ColorIntExt.FromRGB(255, 255, 255));
        }
       
        /// <summary>
        /// Maps each source color to the nearest of black and white.
        /// </summary>
        protected override ColorTransformResult CreateTransformationMap(CancellationToken token = default)
        {
            TransformationMap.Reset();
            RebuildReferencePalette();
            var rgbList = ImageSource.ColorPalette.ToList();
            foreach (var rgb in rgbList)
            {
                TransformationMap.Add(rgb, ColorIntExt.GetNearestColor(rgb, ReferencePalette, this.ColorDistanceEvaluationMode));
            }
            return ColorTransformResult.CreateValidResult();
        }
    }
}