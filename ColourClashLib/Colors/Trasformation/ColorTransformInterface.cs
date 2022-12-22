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
        ColorHistogram SourceColorHistogram { get; }
        ColorPalette SourceColorPalette { get; }
        int SourceColors { get; }

        Dictionary<int, int> TransformationColorMap { get; }
        ColorPalette TransformationColorPalette { get; }
        int TransformationColors{ get; }

        //------------------------------------------------------------
        ColorTransform Type { get; }
        string Description { get; }
        bool Create(int[,]? oDataSource);
        bool Create(ColorHistogram? oColorHistogramSource);
        bool Create(ColorPalette? oColorPaletteSource);
        DitherInterface? Dithering { get; set; }
        int[,]? TransformAndDither(int[,]? oSource);
    }
}
