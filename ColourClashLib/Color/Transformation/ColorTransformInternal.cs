using ColourClashNet.Color.Dithering;
using ColourClashNet.Color.Transformation;
using System;
using System.Collections.Generic;
using System.Text;

namespace ColourClashNet.Color.Transformation
{
    /// <summary>
    /// Factory of the general purpose color reductions that other transformations use internally
    /// (e.g. per tile or per scanline palette reduction).
    /// </summary>
    public static class ColorTransformInternal
    {
        static string sC = nameof(ColorTransformInternal);


        /// <summary>
        /// Creates one of the internal transformations and applies <paramref name="cfg"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="cfg"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="type"/> is not an internal transformation.</exception>
        public static ColorTransformInterface Alloc(ColorTransformConfig cfg, ColorTransformType type)
        {
            string sM = nameof(Alloc);
            if(cfg == null) 
            { 
                throw new ArgumentNullException($"{sC}.{sM} : {nameof(cfg)}");
            }
            ColorTransformInterface transform;
            switch (type)
            {
                case ColorTransformType.ColorReductionClustering:
                    {
                        transform = new ColorTransformClustering();
                    }
                    break;
                case ColorTransformType.ColorReductionFast:
                    {
                        transform = new ColorTransformFast();
                    }
                    break;
                case ColorTransformType.ColorReductionMedianCut:
                    {
                        transform = new ColorTransformMedianCut();
                    }
                    break;
                case ColorTransformType.ColorReductionGenericPalette:
                    {
                        transform = new ColorTransformFixedPalette();
                    }
                    break;
                default:
                    throw new ArgumentException($"{sC}.{sM} : {type} not supported");
            }
            return transform.SetProperties(cfg);
        }

        /// <summary>
        /// Returns the names of the transformations supported by <see cref="Alloc"/>.
        /// </summary>
        public static List<string> GetInternalTransformationList()
        {
            return new List<string>()
            {
                ColorTransformType.ColorReductionClustering.ToString(),
                ColorTransformType.ColorReductionFast.ToString(),
                ColorTransformType.ColorReductionMedianCut.ToString(),
                ColorTransformType.ColorReductionGenericPalette.ToString()
            };
        }
    }
}
