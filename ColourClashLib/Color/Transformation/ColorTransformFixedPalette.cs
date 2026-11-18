using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Transformation
{
    /// <summary>
    /// Maps each source color to the nearest color of a fixed palette (<see cref="ColorTransformBase.ReferencePalette"/>).
    /// <para/>
    /// Hardware specific subclasses provide their palette by overriding <see cref="RebuildReferencePalette"/>.
    /// </summary>
    public class ColorTransformFixedPalette : ColorTransformBase
    {
        /// <summary>
        /// Creates a transformation that uses the configured reference palette.
        /// </summary>
        public ColorTransformFixedPalette()
        {
            Type = ColorTransformType.ColorReductionGenericPalette;
            Description = "Color palette transformation";
        }
        /// <summary>
        /// Sets the fixed palette, overwriting any previous one (fluent).
        /// </summary>
        public ColorTransformFixedPalette WithFixedPalette(List<int> palette)
        {
            config.WithFixedPalette(palette,true);
            return this;
        }

        /// <summary>
        /// Loads the hardware palette; the base implementation keeps the configured palette.
        /// </summary>
        protected virtual void RebuildReferencePalette()
        {
            
        }

        /// <summary>
        /// Maps each source color to the nearest reference palette color.
        /// </summary>
        protected override ColorTransformResult CreateTransformationMap(CancellationToken token = default)
        {
            TransformationMap.Reset();
            RebuildReferencePalette();
            var rgbList = ImageSource.ColorPalette.ToList();

            //Parallel.ForEach(rgbList, rgb =>
            foreach (var rgb in rgbList)    
            {
                TransformationMap.Add(rgb, ColorIntExt.GetNearestColor(rgb, ReferencePalette, this.ColorDistanceEvaluationMode));
            }//);
            return ColorTransformResult.CreateValidResult();
        }
    }
}

