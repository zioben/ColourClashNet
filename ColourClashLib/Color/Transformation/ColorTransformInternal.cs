using ColourClashNet.Color.Dithering;
using ColourClashNet.Color.Transformation;
using System;
using System.Collections.Generic;
using System.Text;

namespace ColourClashNet.Color.Transformation
{
    /// <summary>
    /// Factory of the general purpose color reductions (fast, median cut, clustering) that composite
    /// transformations use as nested steps (e.g. per tile, per chunk or for a base palette).
    /// </summary>
    public static class ColorTransformInternal
    {
        static string sC = nameof(ColorTransformInternal);

        /// <summary>
        /// Creates the color reduction that matches <paramref name="options"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="options"/> is null.</exception>
        public static ColorTransformInterface Alloc(ColorReductionOptions options)
        {
            string sM = nameof(Alloc);
            if (options == null)
                throw new ArgumentNullException($"{sC}.{sM} : {nameof(options)}");
            return ColorTransformBase.CreateColorTransformInterface(options);
        }
    }
}
