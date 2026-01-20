using ColourClashNet.Color;
using ColourClashNet.Color.Transformation;
using ColourClashNet.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        bool Create();
        Task<ImageData?> DitherAsync(ImageData imageReference, ImageData imageProcessed, ColorDistanceEvaluationMode colorEvaluationMode, CancellationToken token=default);



    }
}
