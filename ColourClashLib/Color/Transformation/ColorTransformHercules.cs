using ColourClashNet.Imaging;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Transformation
{
    public class ColorTransformHercules : ColorTransformFixedPalette
    {



        public ColorTransformHercules()
        {
            Type = ColorTransformType.ColorReductionHercules;
            Description = "Reduce color to Hercules palette";
        }

        Palette palette = new();

        protected override void RebuildReferencePalette()
        {
            ReferencePalette.Reset();
            ReferencePalette.Create();
            ReferencePalette.Add(ColorIntExt.FromRGB(0, 0, 0));
            ReferencePalette.Add(ColorIntExt.FromRGB(255, 255, 255));
        }
       
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