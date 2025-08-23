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
        ColorHistogram OutputHistogram { get; }
        ColorPalette OutputPalette { get; }
        Int32 OutputColors { get; }
        ColorTransformationMap ColorTransformationMapper { get; }

        //------------------------------------------------------------
        ColorTransformType Type { get; }
        String Name { get; }
        String Description { get; }
        DitherInterface? Dithering { get; set; }
        ColorTransformInterface? Create(int[,]? oDataSource, ColorPalette? oFixedPaletteSource );
        ColorTransformInterface? Create(ColorHistogram? oColorHistogramSource, ColorPalette? oFixedPaletteSource);
        ColorTransformInterface? Create(ColorPalette? oColorPaletteSource, ColorPalette? oFixedPaletteSource);
        Task<ColorTransformResults> ProcessColorsAsync(int[,]? oSource);
        ColorTransformResults ProcessColors(int[,]? oSource);
        Boolean ProcessAbort();
        ColorTransformInterface? SetProperty(ColorTransformProperties eProperty, object oValue);
        ColorTransformInterface? SetDithering(DitherInterface oDithering);
        //ColorTransformInterface? CreateDither(ColorDithering eDithering);
    }
}
