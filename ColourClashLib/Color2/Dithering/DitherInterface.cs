using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColourClashNet.Color;
using ColourClashNet.Color;

namespace ColourClashNet.Color.Dithering
{
    /// <summary>
    /// Interface for Dithering algorithms
    /// </summary>
    public interface DitherInterface
    {
        ColorDithering Type { get; }
        string Description { get; }
        double DitheringStrenght { get; set; }
        public bool Create();

        Task<int[,]?> DitherAsync(int[,]? oDataSource, int[,]? oDataProcessed, Palette? oDataProcessedPalette, ColorDistanceEvaluationMode eDistanceMode, CancellationToken? oToken);
       // int[,]? Dither(int[,]? oDataSource, int[,]? oDataProcessed, Palette? oDataProcessedPalette, ColorDistanceEvaluationMode eDistanceMode, CancellationToken? oToken);

    }
}
