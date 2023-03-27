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
        ColorHistogram colorHistogram { get; }
        ColorPalette colorPalette { get; }
        int colors { get; }
        ColorTransformationMap colorTransformationMap { get; }

        //------------------------------------------------------------
        ColorTransform type { get; }
        string description { get; }
        bool Create(int[,]? oDataSource, ColorPalette? oFixedPaletteSource );
        bool Create(ColorHistogram? oColorHistogramSource, ColorPalette? oFixedPaletteSource);
        bool Create(ColorPalette? oColorPaletteSource, ColorPalette? oFixedPaletteSource);
        DitherInterface? dithering { get; set; }
        int[,]? TransformAndDither(int[,]? oSource);
    }
}
