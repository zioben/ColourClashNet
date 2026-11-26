using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Transformation
{
    /// <summary>
    /// Base of the transformations that map every source color to the nearest color of a target palette.
    /// <para/>
    /// The target palette is supplied by the derived class through <see cref="GetTargetPalette"/>:
    /// hardware transformations return their own palette (it is never stored in the options),
    /// <see cref="ColorTransformFixedPalette"/> returns <see cref="TransformOptions.UserPalette"/>.
    /// </summary>
    /// <typeparam name="TOptions">Option record of the transformation</typeparam>
    public abstract class ColorTransformPaletteMapping<TOptions> : ColorTransformMapBase<TOptions> where TOptions : TransformOptions, new()
    {
        /// <summary>
        /// Returns the palette the image is mapped to, for the current options.
        /// </summary>
        protected abstract Palette GetTargetPalette();

        /// <summary>
        /// Palette the image is mapped to, for the current options.
        /// </summary>
        public Palette TargetPalette => GetTargetPalette();

        /// <summary>
        /// Maps each source color to the nearest color of <see cref="TargetPalette"/>.
        /// </summary>
        protected override ColorTransformResult CreateTransformationMap(CancellationToken token = default)
        {
            TransformationMap.Reset();
            var targetPalette = GetTargetPalette();
            var rgbList = ImageSource.ColorPalette.ToList();
            foreach (var rgb in rgbList)
            {
                TransformationMap.Add(rgb, ColorIntExt.GetNearestColor(rgb, targetPalette, ColorDistanceEvaluationMode));
            }
            return ColorTransformResult.CreateValidResult();
        }
    }

    /// <summary>
    /// Maps each source color to the nearest color of a user palette (<see cref="TransformOptions.UserPalette"/>).
    /// </summary>
    public class ColorTransformFixedPalette : ColorTransformPaletteMapping<FixedPaletteOptions>
    {
        /// <summary>
        /// Creates the fixed palette transformation.
        /// </summary>
        public ColorTransformFixedPalette()
        {
            Description = "Color palette transformation";
        }

        /// <inheritdoc/>
        protected override Palette GetTargetPalette() => UserPalette;
    }
}
