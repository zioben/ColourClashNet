using System;
using System.Collections.Generic;
using System.Text;

namespace ColourClashNet.Color.Conversion
{
    /// <summary>
    /// Factory helpers for color space converters.
    /// </summary>
    public static class ColorConversionTools
    {
        /// <summary>
        /// Creates an empty converter for the requested color space.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Unsupported <paramref name="colorMode"/>.</exception>
        public static ColorConverterInterface CreateConverter(ColorMode colorMode)
        {
            return colorMode switch
            {
                ColorMode.RGB => new RGB(0, 0, 0),
                ColorMode.HSV => new HSV(0, 0, 0),
                ColorMode.LAB => new LAB(0, 0, 0),
                ColorMode.GRAY => new GRAY(0, 0, 0),
                _ => throw new ArgumentOutOfRangeException(nameof(colorMode), $"Unsupported color mode: {colorMode}"),
            };
        }

        /// <summary>
        /// Creates a converter for the requested color space, initialized from an integer RGB color.
        /// </summary>
        public static ColorConverterInterface CreateConverter(ColorMode colorMode, int rgb)
        {
            var converter = CreateConverter(colorMode);
            converter.FromIntRGB(rgb);
            return converter;
        }
    }
}