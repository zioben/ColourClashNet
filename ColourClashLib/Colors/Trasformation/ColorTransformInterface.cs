using ColourClashLib.Color;
using ColourClashNet.Colors;
using ColourClashNet.Colors.Dithering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Colors.Transformation
{
    public interface ColorTransformInterface
    {

        //------------------------------------------------------------
        ColorHistogram Histogram { get; }
        ColorPalette Palette { get; }
        int Colors { get; }
        ColorTransformationMap ColorTransformationMapper { get; }

        //------------------------------------------------------------
        ColorTransformType Name { get; }
        string Description { get; }
        ColorTransformInterface? Create(int[,]? oDataSource, ColorPalette? oFixedPaletteSource );
        ColorTransformInterface? Create(ColorHistogram? oColorHistogramSource, ColorPalette? oFixedPaletteSource);
        ColorTransformInterface? Create(ColorPalette? oColorPaletteSource, ColorPalette? oFixedPaletteSource);
        DitherInterface? Dithering { get; set; }
        Task<ColorTransformResults> TransformAndDitherAsync(int[,]? oSource);
        ColorTransformResults TransformAndDither(int[,]? oSource);
        bool TransformAbort();
        ColorTransformInterface? SetProperty(ColorTransformProperties eProperty, object oValue);
    }
}
