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
    /// Contract of a dithering algorithm.
    /// <para/>
    /// Dithering compares the color-reduced image with the original one and spreads the
    /// approximation error so that the missing colors are simulated by pixel patterns.
    /// </summary>
    public interface DitherInterface
    {
        /// <summary>Algorithm identifier.</summary>
        ColorDithering Type { get; }

        /// <summary>Human readable description.</summary>
        string Description { get; }

        /// <summary>Dithering strength multiplier (1.0 = nominal strength).</summary>
        double DitheringStrenght { get; set; }

        /// <summary>
        /// Initializes the algorithm internal data (kernels, threshold matrices).
        /// </summary>
        /// <returns>This instance, ready to use.</returns>
        DitherInterface Create();

        /// <summary>
        /// Dithers <paramref name="imageProcessed"/> using <paramref name="imageReference"/> as the target colors.
        /// </summary>
        /// <param name="imageReference">Original (not reduced) image</param>
        /// <param name="imageProcessed">Color-reduced image; its palette is the set of allowed output colors</param>
        /// <param name="colorEvaluationMode">Metric used to pick the nearest palette color</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>The dithering result.</returns>
        ColorTransformResult Dither(ImageData imageReference, ImageData imageProcessed, ColorDistanceEvaluationMode colorEvaluationMode, CancellationToken token=default);
    }
}
