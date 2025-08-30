using ColourClashNet.Color;
using ColourClashNet.Color.Trasformation;
using ColourClashNet.Color;
using ColourClashNet.Color.Dithering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Transformation
{
    public interface ColorTransformInterface
    {

        //------------------------------------------------------------
        Histogram OutputHistogram { get; }
        Palette OutputPalette { get; }
        Int32 OutputColors { get; }
        ColorTransformationMap ColorTransformationMapper { get; }

        //------------------------------------------------------------
        ColorTransformType Type { get; }
        String Name { get; }
        String Description { get; }
        DitherInterface? Dithering { get; set; }
        ColorTransformInterface? Create(int[,]? oDataSource, Palette? oFixedPaletteSource );
        ColorTransformInterface? Create(Histogram? oColorHistogramSource, Palette? oFixedPaletteSource);
        ColorTransformInterface? Create(Palette? oColorPaletteSource, Palette? oFixedPaletteSource);
        Task<ColorTransformResults> ProcessColorsAsync(int[,]? oSource);
        ColorTransformResults ProcessColors(int[,]? oSource );
        Boolean ProcessAbort();
        ColorTransformInterface? SetProperty(ColorTransformProperties eProperty, object oValue);
        ColorTransformInterface? SetDithering(DitherInterface oDithering);
        //ColorTransformInterface? CreateDither(ColorDithering eDithering);
    }
}
