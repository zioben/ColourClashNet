using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColourClashLib.Color;
using ColourClashNet.Colors;

namespace ColourClashNet.Colors.Dithering
{
    public interface DitherInterface
    {
        ColorDithering Type { get; }
        string Description { get; }
        double DitheringStrenght { get; set; }
        public bool Create();

        int[,]? Dither(int[,]? oDataSource, int[,]? oDataProcessed, ColorPalette? oDataProcessedPalette, ColorDistanceEvaluationMode eDistanceMode);

    }
}
