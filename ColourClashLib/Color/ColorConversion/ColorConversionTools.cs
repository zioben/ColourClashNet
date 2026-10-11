using System;
using System.Collections.Generic;
using System.Text;

namespace ColourClashNet.Color.Conversion
{
    public static class ColorConversionTools
    {
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

        public static ColorConverterInterface CreateConverter(ColorMode colorMode, int iRGB)
        {
            var converter = CreateConverter(colorMode);
            converter.FromIntRGB(iRGB);
            return converter;
        }
    }
}