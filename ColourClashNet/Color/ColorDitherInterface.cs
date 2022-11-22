using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Colors
{
    public interface ColorDitherInterface
    { 
        ColorDithering Type { get; }
        string Description { get; }
        double DitheringStrenght { get; set; }
        public bool Create();

        int[,]? Dither(int[,]? oDataSource, int[,]? oDataProcessed, HashSet<int>? oDataProcessedPalette, ColorDistanceEvaluationMode eDistanceMode);

    }
}
